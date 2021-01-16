import machine, utime, ujson, network

from machine import Pin, SPI, ADC

import max7219, httpclient

from secrets import functionsUrl

adc = ADC(0)

def setup_screen():
    spi = SPI(1, baudrate=10000000)
    screen = max7219.Max7219(32, 16, spi, Pin(15))
    screen.brightness(0)
    return screen

# the http-get can take a bit, don't leave the screen blank.
# otherwise, skip screen setup for now, as it clears the screen.
if (machine.reset_cause() == 0): # cold-boot == 0, deepsleep == 5
    screen = setup_screen()
    screen.text( "pls",  0, 0, 1 )
    screen.text( "wait", 0, 8, 1 )
    screen.show()


# print("pulling data")
station = network.WLAN(network.STA_IF)
while not station.isconnected():
    utime.sleep_ms(200)

data = httpclient.get(functionsUrl, timeout=60)
reading = data.json()
# print("done pulling data")

# if waking from a deepsleep, screen setup was deferred
if (machine.reset_cause() != 0):
    screen = setup_screen()

######### scroll in the battery charge levels #########
def multiline_marquee(screen, *lines, width=32):
    start = width + 1
    longest = len(max(lines, key=len))
    extent = 0 - (longest * 8) - 32
    for x in range(start, extent, -1):
        screen.fill(0)
        for y in range(0, len(lines)):
            screen.text(lines[y], x, y * 8, 1)
        screen.show()
        utime.sleep_ms(30)


def to_volts(adc_value, max_adc=1023, max_volts=4.2):
    return str(round((adc_value / max_adc) * max_volts, 1)) + "v"

sensor_battery = reading["Battery"]
screen_battery = adc.read()

multiline_marquee( screen,
                   "screen:" + to_volts(screen_battery),
                   "sensor:" + to_volts(sensor_battery))
########################################################


# screen will already be blank after the marquee
screen.text(str(round(reading["F"], 1))      , 0, 0, 1)
screen.text(str(round(reading["RH"]  )) + "%", 0, 8, 1)


### add some lil horizontal bars for battery life ###
def battery_pixels(adc_reading):
    # (3/4.2)*1023 = 731 => 0 pixels
    # 1023 - 731   = 292 => 8 pixels
    adjusted = adc_reading - 731
    return round(adjusted / 292 * 8)

screen_battery_w = battery_pixels(screen_battery)
screen.hline(24, 12, screen_battery_w, 1)
screen.hline(24, 13, screen_battery_w, 1)

sensor_battery_w = battery_pixels(sensor_battery)
screen.hline(24, 14, sensor_battery_w, 1)
screen.hline(24, 15, sensor_battery_w, 1)
######################################################


#### up arrow if the heater is on, 'pause' if not ####
if (reading["HeaterStateSetTo"] is True):
    screen.pixel(27, 8, 1)
    screen.hline(26, 9, 3, 1)
    screen.hline(25, 10, 5, 1)
else:
    screen.vline(26, 8, 3, 1)
    screen.vline(28, 8, 3, 1)

######################################################

screen.show()
# ^^^ .show() fails if entering machine.*-sleep ASAP for some reason:
utime.sleep_ms(10)

# give time to enter webrepl before deepsleep, just-in-case
machine.lightsleep(20000)
machine.deepsleep(580000)  # 10min = 10 * 60 * 1000 = 600,000 minus 20s
