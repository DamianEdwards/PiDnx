## What is this?
[DNX](https://github.com/aspnet/dnx) is cool. [Windows 10 IoT Core](https://dev.windows.com/en-us/iot) for Raspberry Pi 2 is cool. What happens when you put them together?

The [Raspberry Pi 2](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/) is a US$35 computer, roughly the size of a pack of cigarettes. It has USB, HDMI, and Ethernet, and is a hit in the world of tiny computing and IoT. Previously, it had primarily run Linux derivatives but as of recently, it supports a new version of Windows: Windows 10 IoT Core. In short, this sounds like a great place to see just how cross-platform ASP.NET 5 and DNX are.

The thing standing in the way of trying this until now is the fact that the Raspberry Pi 2 is an [ARM](https://en.wikipedia.org/wiki/ARM_architecture) device, and .NET Core and DNX have only been built for x86/x64. That [recently changed](https://github.com/aspnet/dnx/commit/54ffae496e293c65a5f63323fd43627007db18cf) however so the time is right to give this a go.

## A word of warning
Don't treat this article as a declaration of support, by Microsoft or anybody else, for the scenarios outlined within. We've done the work to **enable** people to have fun with this and you're of course free to log bugs on the relevant [GitHub repos](https://github.com/aspnet) for any issues you run into, but nobody is making any promises or guarantees that bugs will be fixed or further scenarios enabled. We're having fun with it and we want to enable you to have fun too.

## What you'll need
Get yourself the following:
* A [Raspberry Pi 2](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/)
* A 2+ amp USB power pack and micro USB cable to power your Pi
* An HDMI cable and monitor so you can watch the status of your Pi while installing Windows 10 IoT Core
* A micro SD card (8GB+) to install [Windows 10 IoT Core Preview](https://dev.windows.com/iot) on
* A machine running [Windows 10 Preview](http://windowsinsider.com) & [Visual Studio 2015 RC](http://visualstudio.com/)
* A physical network with a cable for both your Raspberry Pi 2 and your Windows 10 machine

## Installing Windows 10 IoT Core
Head over to the Windows 10 IoT Core site and follow [the instructions there to get it installed on your micro SD card](http://ms-iot.github.io/content/en-US/win10/SetupRPI.htm). After you've put the card in the Pi it will start and reboot a few times before finally showing the [default app screen](http://ms-iot.github.io/content/images/DefaultAppRpi2.png). Be patient, this process will take a little while. Continue through the instructions to get the dev tools for Windows 10 IoT Core installed on your Windows 10 machine.

## Setting up your dev environment
There isn't a local console on Windows 10 IoT Core, the only direct interaction supported via keyboard and mouse currently is in [UWP apps](https://dev.windows.com/en-us/windows-apps). You're trying to deploy and run console/server apps using DNX so you'll need to do that via remote PowerShell sessions and UNC. Follow [these instructions](http://ms-iot.github.io/content/en-US/win10/samples/PowerShell.htm) to get remote PowerShell working to your Pi. This will require your Windows 10 machine and Pi to be connected to the same network. I found connecting the Pi and Windows 10 dev machine to the same physical network to be the easiest approach. The IoT Watcher app included with the Windows 10 IoT Core SDK tools makes it simple to see the status of your Pi on the network and quickly connect to its UNC file share for easy file deployment.

At this point I'd recommend a few extra steps to make working with your Pi a little nicer:
* Change the Administrator account password (safety first)
  ```
  net user Administrator [new password]
  ```
* Change the Pi's device name to something other than the default, e.g. Damian-Pi2
  ```
  SetComputerName [new device name]
  ```
* Create a local user account on the Pi and put it in the Administrators group so you can use that for your remote sessions and to set as the user for startup tasks (this will be important to enable remote debugging later on)
  ```
  net user [username] [password] /add
  net localgroup Administrators [username] /add
  ```

Before continuing, confirm you can successfully connect to your Pi via remote PowerShell and UNC using the account you created.

*NOTE: See [this page](http://ms-iot.github.io/content/en-US/win10/tools/CommandLineUtils.htm) for a useful list of command line utilities supported on Windows 10 IoT Core*

## Installing the DNX for Windows ARM
As mentioned earlier, the Raspberry Pi 2 is an ARM device, so we need a version of the DNX compiled a) for ARM, and b) against the Win32 API set used by Windows 10 IoT Core (OneCore). We can install that on our development machine using `dnvm`, then later we'll package it up with our application using `dnu publish`.

Open a command prompt on your development machine and run the following commands:

```
dnvm update-self
```

```
dnvm install latest -r coreclr -arch ARM -u
```

```
dnvm install latest -r coreclr -arch x86 -u
```

```
dnvm install latest -r coreclr -arch x64 -u
```

```
dnvm install latest -r clr -arch x86 -u
```

```
dnvm install latest -r clr -arch x64 -u
```

Be sure to include the `-u` switch to use the unstable feed as at the time of writing, we haven't published the ARM DNX build to the stable feed (nuget.org).

After this, you should be able to run `dnvm list` and see the ARM DNX in the list of installed versions (it's the 2nd last one in my list below):

```
C:\> dnvm list

Active Version           Runtime Architecture Location                      Alias
------ -------           ------- ------------ --------                      -----
       1.0.0-beta4       clr     x64          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta4       clr     x86          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta4       coreclr x64          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta4       coreclr x86          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta5       clr     x64          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta5       clr     x86          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta5       coreclr x64          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta5       coreclr x86          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta6-12032 clr     x86          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta6-12032 coreclr x86          C:\Users\Damian\.dnx\runtimes
  *    1.0.0-beta6-12082 clr     x86          C:\Users\Damian\.dnx\runtimes default
       1.0.0-beta6-12082 coreclr arm          C:\Users\Damian\.dnx\runtimes
       1.0.0-beta6-12082 coreclr x86          C:\Users\Damian\.dnx\runtimes

 C:\>
```

## Creating and publishing your DNX application
Let's create an application:
1. Use Visual Studio 2015 RC to create a new ASP.NET 5 application using the Empty project template.
1. Open the solution's `global.json` file and change the `sdk` property to match the version you installed using `dnvm` in the previous section (hint: it's listed in the output of `dnvm list` on your dev machine).
1. Close and re-open the solution to allow Visual Studio to load the correct version of DNX based on your change to `global.json`.
1. Open the project's `project.json` file and change the versions of the listed dependencies from "1.0.0-beta4" to "1.0.0-*". This will ensure the latest versions of the packages are restored.
1. Go to the root of the *solution folder* for the project and add a `nuget.config` file, pasting in the following configuration which enables the ASP.NET 5 dev feed, containing the latest drops from our build servers:
  ``` xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
    <packageSources>
      <add key="AspNetVNext" value="https://www.myget.org/F/aspnetvnext/api/v2" />
      <add key="NuGet" value="https://nuget.org/api/v2/" />
    </packageSources>
  </configuration>
  ```
1. Force a package restore either by touching and saving the `project.json` file, or right-mouse clicking on the project in Solution Explorer and choosing "Restore Packages". Open the References node and ensure all the packages listed are from the same release (e.g. 1.0.0-**beta6**-[build number] at the time of writing).
1. Run the application (Ctrl+F5) and ensure you see the "Hello World" message in your browser.
1. At this stage, the application will run on localhost, but when on our Pi we'll want it to respond to remote requests, so let's make it easier to change the HTTP addresses it binds to after we publish it by externalizing the server configuration.
1. Open the `project.json` file and change the `"web"` command to get its arguments from a `hosting.ini` file:
  ``` JSON
  "web": "Microsoft.AspNet.Hosting --config hosting.ini"
  ```
1. Create the `hosting.ini` file in the root of the project and paste the following configuration values in:
  ``` INI
  server=Microsoft.AspNet.Server.WebListener
  server.urls=http://localhost:5000
  ```
1. Select the `web` command from the Debug/Launch button on the toolbar and run the application again. This should launch a command window with a message stating the web server was successfully started.
1. Open your browser and navigate to http://localhost:5000/ to ensure your application works using the `WebListener` server with the new configuration.

Now we have an application ready to package up for deployment to the Pi.

1. Open a command prompt on your dev machine and navigate to the folder containing your application.
1. Run `dnvm list` and note the version number for the ARM DNX you installed ealier, e.g.  1.0.0-beta6-12082
1. Run the following command to compile and publish the application, along with the DNX, to a folder on your dev machine. Ensure the version from the previous step matches the one in the value you pass to the `--runtime` argument. The value should exactly match the runtime's folder name in your `%USERPROFILE%\.dnx\runtimes` folder:
  ```
  dnu publish --out C:\publish\DnxPi --no-source --runtime dnx-coreclr-win-arm.1.0.0-beta6-12082
  ```
1. Navigate to the publish location and dive down into the structure to find the `root` folder in the package folder your application was compiled into, e.g. `C:\publish\DnxPi\approot\packages\DnxPi\1.0.0\root`. This folder should contain the `hosting.ini` file you created earlier.
1. Open the `hosting.ini` file and change the URL the application listens on to match all host names:
  ```
  server.urls=http://*:5000
  ```
1. Open the UNC share to your Pi in Explorer and copy the published app over to a suitable place, e.g. in the `C:\PROGRAMS` folder. *Tip: Right-mouse click on your Pi in the IoT Watcher app to quickly open the UNC share*

## Opening a port in the firewall on your Pi
You're going to be browsing to your DNX application from a machine other than the Pi so you'll need to enable remote access by adding a rule to the firewall:
1. Open a remote PowerShell session to your Pi with an account with Administrator rights, e.g. the account you created eariler.
1. Run the following command:
   ```
   netsh advfirewall firewall add rule name="DNX Web Server port" dir=in action=allow protocol=TCP localport=5000
   ```

## Running your DNX application on the Pi
Your application is now deployed and ready to go so let's run it!
1. Open a remote PowerShell session to your Pi with an account with Administrator rights, e.g. the account you created eariler.
1. Navigate to the application folder on the Pi, e.g. `C:\PROGRAMS\DnxPi`
1. Run `web.cmd` and wait... you should see a message saying the server started after 5-10 seconds
1. Open a browser and point it at your Pi and port 5000, e.g. http://Damian-Pi2:5000/

At this point you should see the familiar "Hello World!" message in your browser. If you don't, I'm sorry. Either you made a mistake in following this guide, I made a mistake in writing it, or something environmental (maybe cosmic rays?) is spoiling your fun. Enjoy debugging it and when you figure it out, [let me know](https://twitter.com/DamianEdwards), so I can update this guide for others.

If you hit refresh a few times the server will stop responding. There's an issue right now with the `WebListener` server when running on ARM that we're yet to investigate.

What you've done is about as far as we've got trying out DNX/ASP.NET 5 development on Windows 10 IoT Core, so if you go any further you're in totally uncharted territory. If you find issues along the way, please log them and we'll do our best to check them out.

If you want a *very slightly* more interesting application to run on your Pi try [this one](https://github.com/DamianEdwards/PiDnx).

## What about debugging?
We've managed to successfully remote debug a DNC application running on the Pi but the set up is a little tricky so stay tuned for an update on how to enable that sorcery.
