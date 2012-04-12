#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <windows.h>
#include <stdio.h>
#include <stdlib.h>

#include <winsock2.h>
#include <ws2tcpip.h>

#pragma comment(lib, "Ws2_32.lib")

#include "vmulticlient.h"

// settings
#define DEFAULT_PORT "27015"

//
// Function prototypes
//

int
SendHidRequests(
    pvmulti_client vmulti,
    BYTE requestType
    );

//
// Implementation
//

void
Usage(void)
{
    printf("Usage: testvmulti </multitouch | /mouse | /digitizer | /joystick | /keyboard | /message>\n");
}

INT __cdecl
main(
    int argc,
    PCHAR argv[]
    )
{
    BYTE   reportId;
    pvmulti_client vmulti;

    UNREFERENCED_PARAMETER(argv);

    //
    // Parse command line
    //

    if (argc == 1)
    {
        Usage();
        return 1;
    }
    if (strcmp(argv[1], "/multitouch") == 0)
    {
        reportId = REPORTID_MTOUCH;
    }
    else if (strcmp(argv[1], "/mouse") == 0)
    {
        reportId = REPORTID_MOUSE;
    }
    else if (strcmp(argv[1], "/digitizer") == 0)
    {
        reportId = REPORTID_DIGI;
    }
    else if (strcmp(argv[1], "/joystick") == 0)
    {
        reportId = REPORTID_JOYSTICK;
    }
    else if (strcmp(argv[1], "/keyboard") == 0)
    {
        reportId = REPORTID_KEYBOARD;
    }
    else if (strcmp(argv[1], "/message") == 0)
    {
        reportId = REPORTID_MESSAGE;
    }
    else
    {
        Usage();
        return 1;
    }

    //
    // File device
    //

    vmulti = vmulti_alloc();

    if (vmulti == NULL)
    {
        return 2;
    }

    if (!vmulti_connect(vmulti))
    {
        vmulti_free(vmulti);
        return 3;
    }

    printf("...sending request(s) to our device\n");
    SendHidRequests(vmulti, reportId);

    vmulti_disconnect(vmulti);

    vmulti_free(vmulti);

    return 0;
}

int
SendHidRequests(
    pvmulti_client vmulti,
    BYTE requestType
    )
{

	////--------------------------------------------
	// setup socket stuff

	WSADATA wsaData;

	int iResult;
	#define DEFAULT_BUFLEN 12

	char recvbuf[DEFAULT_BUFLEN];
	int iSendResult;
	int recvbuflen = DEFAULT_BUFLEN;
	struct addrinfo *result = NULL;
    struct addrinfo *ptr = NULL;
    struct addrinfo hints;

	SOCKET ls = INVALID_SOCKET;
	SOCKET client;

	// Initialize Winsock
	iResult = WSAStartup(MAKEWORD(2,2), &wsaData);
	if (iResult != 0) {
		printf("WSAStartup failed: %d\n", iResult);
		return 1;
	} else {
		printf("WSAStartup okay\n");
	}

	ZeroMemory(&hints, sizeof (hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;

	// Resolve the local address and port to be used by the server
	iResult = getaddrinfo(NULL, DEFAULT_PORT, &hints, &result);
	if (iResult != 0) {
		printf("getaddrinfo failed: %d\n", iResult);
		WSACleanup();
		exit(-1); // bad
	}

	

	ls = socket(result->ai_family, result->ai_socktype, result->ai_protocol);

	if (ls == INVALID_SOCKET) {
		printf("Error connecting to socket(): %ld\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();
		exit(-1);
	}

	iResult = bind(ls, result->ai_addr, (int)result->ai_addrlen);
    if (iResult == SOCKET_ERROR) {
        printf("bind failed with error: %d\n", WSAGetLastError());
        freeaddrinfo(result);
        closesocket(ls);
        WSACleanup();
        exit(-1);
    }

	freeaddrinfo(result);

	if ( listen( ls, SOMAXCONN ) == SOCKET_ERROR ) {
		printf( "Listen failed with error: %ld\n", WSAGetLastError() );
		closesocket(ls);
		WSACleanup();
		exit(-1);
	}

	client = INVALID_SOCKET;

	printf("about to accept\n");
	// accept a single client
	client = accept(ls, NULL, NULL);
	if (ls == INVALID_SOCKET) {
		printf("accept failed: %d\n", WSAGetLastError());
		closesocket(ls);
		WSACleanup();
		exit(-1);
	}

	

	// Receive until the client shuts down the connection
	do {
		int i = 0;
		iResult = recv(client, recvbuf, recvbuflen, 0);
		if (iResult > 0) {
			USHORT buttons = 0;
            BYTE hat = 0, x = 0, y = 128, rx = 128, ry = 64, throttle = 0;
			//printf("Bytes received: %d\n", iResult);

			// do stuff
			
			if(iResult < 12){
				printf("Expected at least 12 bytes.\n");
				exit(-1);
			}

			for(i = 0; i < 12; i++){
				printf("%u ", (unsigned char) recvbuf[i]);
			}
			printf("---\n");


			x = (BYTE)((((unsigned int) recvbuf[1]) + 128) % 256); // (BYTE) recvbuf[1];
			y = (BYTE)((((unsigned int) recvbuf[3]) + 128) % 256);// (BYTE) recvbuf[3];
			rx = (BYTE) recvbuf[5];
			ry = (BYTE) recvbuf[7];

			buttons = 0;
			buttons |= ((recvbuf[9] & 4) && 1) << 0; // cross
			buttons |= ((recvbuf[9] & 32) && 1) << 1; // square
			buttons |= ((recvbuf[9] & 8) && 1) << 2; // circle
			buttons |= ((recvbuf[9] & 16) && 1) << 3; // triangle
			buttons |= ((recvbuf[8] & 128) && 1) << 4; // L1
			buttons |= ((recvbuf[8] & 16) && 1) << 5; // R1
			buttons |= ((recvbuf[8] & 32) && 1) << 6; // L3
			buttons |= ((recvbuf[8] & 4) && 1) << 7; // R3

			throttle = ((recvbuf[8] & 8) && 1) ? 128 : 0;

			//
            // Send the joystick report
            //
            
            //printf("Sending joystick report\n");

            vmulti_update_joystick(vmulti, buttons, hat, x, y, rx, ry, throttle);

            /*buttons =  rand() | ((rand()&1) << 15); //rand() | rand() << 15 | rand() % 4 << 30;
            hat++;
            x++;
            y++;
            rx++;
            ry--;
            throttle++;*/

		} else if (iResult == 0)
			printf("Connection closing...\n");
		else {
			printf("recv failed: %d\n", WSAGetLastError());
			closesocket(client);
			WSACleanup();
			exit(-1);
		}
	} while (iResult > 0);


	return 0;

	////--------------------------------------------

    switch (requestType)
    {
        case REPORTID_MTOUCH:
        {
            //
            // Send the multitouch reports
            //
            
            BYTE i;
            BYTE actualCount = 4; // set whatever number you want, lower than MULTI_MAX_COUNT
            PTOUCH pTouch = (PTOUCH)malloc(actualCount * sizeof(TOUCH));

            printf("Sending multitouch report\n");
            Sleep(3000);

            for (i = 0; i < actualCount; i++)
            {
                pTouch[i].ContactID = i;
                pTouch[i].Status = MULTI_CONFIDENCE_BIT | MULTI_IN_RANGE_BIT | MULTI_TIPSWITCH_BIT;
                pTouch[i].XValue = (i + 1) * 1000;
                pTouch[i].YValue = (i + 1) * 1500 + 5000;
                pTouch[i].Width = 20;
                pTouch[i].Height = 30;
            }

            if (!vmulti_update_multitouch(vmulti, pTouch, actualCount))
              printf("vmulti_update_multitouch TOUCH_DOWN FAILED\n");
              
            for (i = 0; i < actualCount; i++)
            {
                pTouch[i].XValue += 1000;
                pTouch[i].YValue += 1000;
            }              

            if (!vmulti_update_multitouch(vmulti, pTouch, actualCount))
                printf("vmulti_update_multitouch TOUCH_MOVE FAILED\n");

            for (i = 0; i < actualCount; i++)
              pTouch[i].Status = 0;

            if (!vmulti_update_multitouch(vmulti, pTouch, actualCount))
                printf("vmulti_update_multitouch TOUCH_UP FAILED\n");
                        
            free(pTouch);
            
            break;
        }

        case REPORTID_MOUSE:
            //
            // Send the mouse report
            //
            printf("Sending mouse report\n");
            vmulti_update_mouse(vmulti, 0, 1000, 10000, 0);
            break;

        case REPORTID_DIGI:
            //
            // Send the digitizer reports
            //
            printf("Sending digitizer report\n");
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT, 1000, 10000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT, 1000, 12000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT, 1000, 14000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT | DIGI_TIPSWITCH_BIT, 1000, 16000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT | DIGI_TIPSWITCH_BIT, 1000, 18000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT | DIGI_TIPSWITCH_BIT, 1000, 20000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT | DIGI_TIPSWITCH_BIT, 2000, 20000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT | DIGI_TIPSWITCH_BIT, 3000, 20000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT, 3000, 20000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT, 3000, 15000);
            Sleep(100);
            vmulti_update_digi(vmulti, DIGI_IN_RANGE_BIT, 3000, 10000);
            vmulti_update_digi(vmulti, 0, 3000, 10000);
            break;

        case REPORTID_JOYSTICK:
        {
			
            
            break;
        }
        
        case REPORTID_KEYBOARD:
        {
            //
            // Send the keyboard report
            //

            // See http://www.usb.org/developers/devclass_docs/Hut1_11.pdf
            // for a list of key codes            
                        
            BYTE shiftKeys = KBD_LGUI_BIT;
            BYTE keyCodes[KBD_KEY_CODES] = {0, 0, 0, 0, 0, 0};
            printf("Sending keyboard report\n");

            // Windows key
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            shiftKeys = 0;
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            Sleep(100);
            
            // 'Hello'
            shiftKeys = KBD_LSHIFT_BIT;
            keyCodes[0] = 0x0b; // 'h'
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            shiftKeys = 0;
            keyCodes[0] = 0x08; // 'e'
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            keyCodes[0] = 0x0f; // 'l'
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            keyCodes[0] = 0x0;
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            keyCodes[0] = 0x0f; // 'l'
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            keyCodes[0] = 0x12; // 'o'
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            keyCodes[0] = 0x0;
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            
            // Toggle caps lock
            keyCodes[0] = 0x39; // caps lock
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
            keyCodes[0] = 0x0;
            vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);

            break;
        }

        case REPORTID_MESSAGE:
        {
            VMultiMessageReport report;

            printf("Writing vendor message report\n");

            memcpy(report.Message, "Hello VMulti\x00", 13);

            if (vmulti_write_message(vmulti, &report))
            {
                memset(&report, 0, sizeof(report));
                printf("Reading vendor message report\n");
                if (vmulti_read_message(vmulti, &report))
                {
                    printf("Success!\n    ");
                    printf(report.Message);
                }
            }

            break;
        }
    }
}