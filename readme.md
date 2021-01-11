
A quick and dirty micropython project, my first time playing with [these](https://www.amazon.com/gp/product/B088LQMVRQ) MAX7219 LED Matrix displays.

---

[MicroPython](http://docs.micropython.org/en/latest/esp8266/tutorial/intro.html#intro) on a low-cost ESP8266 is a great way to get started with "iot" sorts of projects.  Copy the latest firmware over, log into your wifi, set up the ["WebREPL"](http://docs.micropython.org/en/latest/esp8266/quickref.html#webrepl-web-browser-interactive-prompt) and voila, you are set to copy and update your programs wirelessly.  

---

Project `Si7021_8266` reads sensor data every 10 minutes and pushes it to an http [Azure Function](https://azure.microsoft.com/en-us/services/functions/), which is free up to 1 million triggers per month. 

In turn, `Functions` project logs the readings and conditionally triggers IFTTT based on temperature trends.

Project `Max7219_8266` downloads & displays the latest data from `Functions` project every 10 minutes.

I'm using Azure Functions for simplicity and [Blob Storage vs Table Storage](https://docs.microsoft.com/en-us/azure/storage/common/storage-introduction#core-storage-services) again for simplicity but also because i'm a sucker for json.

---

STLs for mounting the LED matrixes can be found [here](https://github.com/McNerdius/3DP/tree/master/LED%20Matrix%2016x32/STL)

Wiring diagrams to come, but the hookups for the LED matrix displays match [this](https://github.com/joewez/WifiMarquee); the si7021 & OLED miniscreen use default i2c lines (D1/D2 on the [wemos d1 mini](https://www.wemos.cc/en/latest/d1/d1_mini.html)); and note you'll need to connect D0 to RST on the D1 mini for deep sleep to work.

---

other hardware in use:

Display:

* [WeMos D1 Mini (clone)](https://www.amazon.com/gp/product/B08FQYZX37).  Both the brains and the least expensive part, i love these things.  Quite versatile and inexpensive.  No need for more expensive, higher-spec options when getting started in this realm.
* [Adafruit PowerBoost 1000 Charger](https://www.adafruit.com/product/2465).  I imagine the matrix displays are fairly power-hungry and i had one of these on hand.
  
Sensor unit:

* [Si7021](https://www.adafruit.com/product/3251) humidity/sensor unit.  Not the newer "STEMMA QT" form factor, but otherwise the same hardware.  Used this to feed data to the display unit.
* [Tiny OLED screen](https://www.amazon.com/gp/product/B07D9H83R4/).

---

libraries in use:

* [jgbrown32's ESP8266_MAX7219](https://github.com/jgbrown32/ESP8266_MAX7219) library for the led matrix itself.
* [chrisbalmer's micropython-si7021](https://github.com/chrisbalmer/micropython-si7021) library for the humidity/sensor unit.
* [ssd1306](https://github.com/micropython/micropython/blob/master/drivers/display/ssd1306.py) for the tiny OLED display.
* `httpclient.py` - can't remember/trace down where i picked this one up.

---

![pic 1](1.jpg)


![pic 2](2.jpg)