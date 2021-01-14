import machine, ujson, utime, network
from machine import Pin, I2C, ADC

import httpclient, si7021, ssd1306

from secrets import functionsUrl

i2c = I2C(scl=Pin(5), sda=Pin(4), freq=400000)

screen = ssd1306.SSD1306_I2C(128, 32, i2c, 60)
screen.contrast(0)

temp_sensor = si7021.Si7021(i2c)

adc = ADC(0)

reading = {
    "F": round(si7021.convert_celcius_to_fahrenheit(temp_sensor.temperature),
               1),
    "RH": round(temp_sensor.relative_humidity, 1),
    "Battery": adc.read()
}

temp = str(reading["F"])
rh = str(reading["RH"])
battery = str(reading["Battery"])

screen.fill(0)

screen.text(temp + " f", 2, 2, 1)
screen.text(rh + " rh", 2, 12, 1)
screen.text(battery, 2, 22, 1)

screen.text("pushing", 68, 12, 1)
screen.text("data...", 68, 20, 1)

screen.show()

# print("pushing data: " + ujson.dumps(reading))

station = network.WLAN(network.STA_IF)
while not station.isconnected():
    utime.sleep_ms(200)

httpclient.post(functionsUrl, json=reading, timeout=60)

# print("done !")

screen.fill_rect(68, 12, 56, 16, 0)
screen.text("data", 68, 12, 1)
screen.text("pushed!", 68, 20, 1)
screen.show()
utime.sleep_ms(10)

# give time to enter webrepl before deepsleep
machine.lightsleep(20000)
screen.fill(0)
screen.show()
machine.deepsleep(580000)  # 10min = 10 * 60 * 1000 = 600,000 minus 20s
