import machine, utime, ujson, network

from machine import Pin, SPI

import max7219, httpclient

from secrets import functionsUrl

spi = SPI(1, baudrate=10000000)

screen = max7219.Max7219(32, 16, spi, Pin(15))
screen.brightness(0)

screen.text("pls",0,8,1)
screen.text("wait",0,0,1)
screen.show()

# print("pulling data")

station = network.WLAN(network.STA_IF)
while not station.isconnected():
    utime.sleep_ms(200)

data = httpclient.get(functionsUrl, timeout=60)
reading = data.json()

# print("done pulling data")

rh = round(reading["RH"])
temp = round(reading["F"],1)

screen.fill(0)
screen.text(str(rh) + "% rh", 0, 0, 1)
screen.text(str(temp) + " f", 0, 8, 1)
screen.show()

# ^^^ .show() fails if entering machine.*-sleep ASAP for some reason:
utime.sleep_ms(10) 

# give time to enter webrepl before deepsleep, just-in-case
machine.lightsleep(20000)
machine.deepsleep(580000)  # 10min = 10 * 60 * 1000 = 600,000 minus 20s
