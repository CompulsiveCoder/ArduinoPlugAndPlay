#include <Arduino.h>
#include <EEPROM.h>

#include <duinocom.h>

#include "Common.h"

#define VERSION "1-0-0-1"

int value = 0;

#define valueIsSetFlagAddress 10
#define valueAddress 13

void setup()
{
  Serial.begin(9600);

  Serial.println("Starting example sketch for MQTT bridge");
  
  setupValue();
}

void loop()
{
  loopNumber++;

  serialPrintLoopHeader();

  checkCommand();

  serialPrintData();

  serialPrintLoopFooter();

  delay(1);
}

/* Commands */
void checkCommand()
{
  if (isDebugMode)
  {
    Serial.println("Checking incoming serial commands");
  }

  if (checkMsgReady())
  {
    char* msg = getMsg();
        
    char letter = msg[0];

    int length = strlen(msg);

    Serial.print("Received message: ");
    Serial.println(msg);

    switch (letter)
    {
      case 'V':
        setValue(msg);
        break;
    }

    forceSerialOutput();
  }
  
  delay(1);
}

void setupValue()
{
  bool eepromIsSet = EEPROM.read(valueIsSetFlagAddress) == 99;

  if (eepromIsSet)
  {
    if (isDebugMode)
    	Serial.println("EEPROM read interval value has been set. Loading.");

    value = getValue();
  }
  else
  {
    if (isDebugMode)
      Serial.println("EEPROM read interval value has not been set. Using defaults.");
  }
}

void setValue(char* msg)
{
    int value = readInt(msg, 1, strlen(msg)-1);

    setValue(value);
}

void setValue(long newValue)
{
  if (isDebugMode)
  {
    Serial.print("Set sensor reading interval: ");
    Serial.println(newValue);
  }

  EEPROMWriteLong(valueAddress, newValue);

  setEEPROMValueIsSetFlag();

  value = newValue; 
}

long getValue()
{
  long value = EEPROMReadLong(valueAddress);

  if (value == 0
      || value == 255)
    return value;
  else
  {
    if (isDebugMode)
    {
      Serial.print("Read interval found in EEPROM: ");
      Serial.println(value);
    }

    return value;
  }
}

void setEEPROMValueIsSetFlag()
{
  if (EEPROM.read(valueIsSetFlagAddress) != 99)
    EEPROM.write(valueIsSetFlagAddress, 99);
}

/* Serial Output */
void serialPrintData()
{
  bool isTimeToPrintData = lastSerialOutputTime + secondsToMilliseconds(serialOutputIntervalInSeconds) < millis()
      || lastSerialOutputTime == 0;

  bool isReadyToPrintData = isTimeToPrintData;

  if (isReadyToPrintData)
  {
    if (isDebugMode)
      Serial.println("Ready to serial print data");
  
    Serial.print("D;"); // This prefix indicates that the line contains data.
    Serial.print("V:");
    Serial.print(value);
    Serial.print(";");
    Serial.print("S:");
    Serial.print(millis()/1000);
    Serial.print(";");
    Serial.print("Z:");
    Serial.print(VERSION);
    Serial.print(";;");
    Serial.println();
    
    lastSerialOutputTime = millis();
  }
}
