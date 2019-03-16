#ifndef duinocom_H_
#define duinocom_H_

const int MAX_MSG_LENGTH = 10;


bool checkMsgReady();

char* getMsg();

int getMsgLength();

void printMsg(char msg[MAX_MSG_LENGTH]);

void clearMsg(char msg[MAX_MSG_LENGTH]);

void identify();

char getCmdChar(char msg[MAX_MSG_LENGTH], int characterPosition);

int readInt(char msg[MAX_MSG_LENGTH], int startPosition, int digitCount);

#endif
