# This file is executed on every boot (including wake-boot from deepsleep)
import esp
esp.osdebug(None)
uos.dupterm(None, 1)  # disable REPL on UART(0)

import micropython
micropython.opt_level(2)

# kill this if you're desperate to save battery 
# (and are sure you don't want to use webrepl)
import webrepl
webrepl.start()

import gc
gc.collect()
