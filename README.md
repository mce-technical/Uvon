# **Uvon**
**_Windows application that is written on C# using WPF to control robot._**

# Usage
### Open application: there are a few options to connect to your host. 
1. You can select one of the ip addresses in list and click **_Connect_**. In the start application scans for the first 15 possible devices.
2. You can do additional scan by your custom search interval by writting interval in text box and clicking on **_Scan WLAN_** button.
![](https://github.com/mce-technical/Uvon/blob/Uvon_Desktop/Screenshots/start_page.png)
3. You can enter Ip address of your host and click on **_Connect_**.
![](https://github.com/mce-technical/Uvon/blob/Uvon_Desktop/Screenshots/connect.png)
### After connection you will see the robot's control panel.
1. In control panel you can get camera preview from server/robot, start line tracking or control robot's movements manually.
![](https://github.com/mce-technical/Uvon/blob/Uvon_Desktop/Screenshots/control_page.png)
2. Also you can turn ON/OFF UV lamp. To turn ON UV lamp you need to enable UV lamp's all two levels of power.
![](https://github.com/mce-technical/Uvon/blob/Uvon_Desktop/Screenshots/enabe_uv.png)
3. You can read robot's control instruction in Informational page. To open it you need to click on the _Info_ button.
![](https://github.com/mce-technical/Uvon/blob/Uvon_Desktop/Screenshots/info_page.png)
# Structur

### The application is made of two pages.
1. The fisrt is the start page where you can choose connection method and connect to your host/robot.
2. In the second page you can see camera preview, control robot movement and enable or disable UV lamp.
