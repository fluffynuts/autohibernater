# autohibernater  
a small win32 service to marshal email alert events from an RCT ups into hibernation events  
  
# What you will need  
1) ViewPower (http://www.power-software-download.com/viewpower.html)  
2) An RCT UPS (or another which works with ViewPower -- not sure if there even are?)  
3) Your PC actually on the UPS...  
4) A USB cable from your UPS to your PC (long USB printer cable)  
5) Configure ViewPower:  
    a) Configure the SMTP server that ViewPower will use  
        - configuration ViewPower  
            - E-Mail  
                - SMTP Server settings >  
                    SMTP server: localhost  
                    Port: 2525 (default, configurable in AutoHibernater.exe.config)  
                    The other settings don't matter, but I'd suggest NOT enabling  
                        SMTP authentication  
    b) Configure event messages:  
        - configuration ViewPower  
            - Event actions  
                - In the list, configure email alerts for:  
                    "AC failure"  
                    "AC recovery"  
                    - just tick the email address option (should appear after   
                        configuration above)  
6) Installed AutoHibernator service:  
    a) Build!  
    b) Copy binaries from AutoHibernater/bin/Debug to a location of your choice  
    c) open an adminstrator console, cd to where you put the binaries and type:  
            AutoHibernater.exe -i  
        You should see a message about the service being registered. To start it,  
        do:  
            net start autohibernater  
  
That should be it.  
  
# What you should experience:  
1) AC power loss  
    - service waits for the GracePeriod (default 60 seconds) then hibernates your machine  
2) AC power recovery within GracePeriod  
    - service cancels pending hibernation and your machine should not hibernate  
  
# Configuration:  
  
You can set the port for the SMTP server to listen on as well as the grace period  
(in seconds) in the AutoHibernater.exe.config file. The SMTP server ONLY listens  
on the localhost loopback device.  

