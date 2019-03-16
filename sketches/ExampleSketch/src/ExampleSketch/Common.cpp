#include <Arduino.h>
#include <EEPROM.h>

#include "Common.h"

const int ANALOG_MAX = 1023;

long lastSerialOutputTime = 0;
long serialOutputIntervalInSeconds = 1;

bool isDebugMode = false;

int loopNumber = 0;

void serialPrintLoopHeader()
{
  if (isDebugMode)
  {
    Serial.println("==========");
    Serial.print("Start Loop: ");
    Serial.println(loopNumber);
    Serial.print("Time: ");
    Serial.print(millisecondsToSecondsWithDecimal(millis()));
    Serial.println(" seconds");
    Serial.println("");
  }
}

void serialPrintLoopFooter()
{
  if (isDebugMode)
  {
    Serial.print("End Loop: ");
    Serial.println(loopNumber);
    Serial.println("==========");
    Serial.println("");
  }
}


void EEPROMWriteLong(int address, long value)
{
      //Decomposition from a long to 4 bytes by using bitshift.
      //One = Most significant -> Four = Least significant byte
      byte four = (value & 0xFF);
      byte three = ((value >> 8) & 0xFF);
      byte two = ((value >> 16) & 0xFF);
      byte one = ((value >> 24) & 0xFF);

      //Write the 4 bytes into the eeprom memory.
      EEPROM.write(address, four);
      EEPROM.write(address + 1, three);
      EEPROM.write(address + 2, two);
      EEPROM.write(address + 3, one);
}

long EEPROMReadLong(int address)
{
      //Read the 4 bytes from the eeprom memory.
      long four = EEPROM.read(address);
      long three = EEPROM.read(address + 1);
      long two = EEPROM.read(address + 2);
      long one = EEPROM.read(address + 3);

      //Return the recomposed long by using bitshift.
      return ((four << 0) & 0xFF) + ((three << 8) & 0xFFFF) + ((two << 16) & 0xFFFFFF) + ((one << 24) & 0xFFFFFFFF);
}

long secondsToMilliseconds(int seconds)
{
  return seconds * 1000;
}

float millisecondsToSecondsWithDecimal(int milliseconds)
{
  return float(milliseconds) / float(1000);
}

void forceSerialOutput()
{
    // Reset the last serial output time 
    lastSerialOutputTime = millis()-secondsToMilliseconds(serialOutputIntervalInSeconds);
}

