# **Uvon**
**_Android application that is written on C# using Xamarin Forms to control robot._**

# Structur

### The application is made of four pages. 
1. The first is the start and connection page.
2. The second is the page where you can see available devices after scanning the local network. From here you can add any device you        want to your favourite devices page.
3. The next is the page where you can see your favourite devices. Here you can delete any device you want.
4. The last page is the robot control page. Here you can see robot's camera preview, enable/disable robot's motor drivers, drive it, enable/disable UV light and see some information about robot state.

# Usage
### Open application: In the start page (1) there are a few options to connect to your host. 
1. If you know your server/robot's ip address, you can enter it and click on **_Submit_** button.
![](https://github.com/mce-technical/Uvon/blob/master/Screenshots/submit.png)
2. If you don't know your server/robot's ip address, you can enter number of checking ip addresses, and click on **_Scan_** button. The application will begin to check ip addresses in sequence started by the first ip in LAN (192.168.X.1) over the entered number.
![](https://github.com/mce-technical/Uvon/blob/master/Screenshots/scan.png).

3. After connection is open the application will redirect you to the control page.

![](https://github.com/mce-technical/Uvon/blob/master/Screenshots/control.png)

### Side bar
1. The first element in side bar is the connection page which is opened when application starts.
2. The second is **_My List_** page, where you can add ip addresses (servers/robots) and connect them.
![](https://github.com/mce-technical/Uvon/blob/master/Screenshots/mylist.png)



