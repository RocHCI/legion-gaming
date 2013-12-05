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
	// Anna
	int i = 0;
	int j = 0;

	int old_pos_x = 0;
	int old_pos_y = 0;
	int new_pos_x = 0;
	int new_pos_y = 0;
	int new_pos_rx = 0;
	int new_pos_ry = 0;
	
	int mouse_x = 0;
	int mouse_y = 0;
	int mouse_cnt = 0;
	int mouse_cntmax = 0;
	int kb_cnt = 0;
	int kb_cntmax = 5;
	int screen_x = 0;
	int screen_y = 0;
	//int screen_w = 60000;
	//int screen_h = 30000;
	int screen_w = 32725;
	int screen_h = 32600;
	// center
	int screen_cx = screen_x + (screen_w / 2);
	int screen_cy = screen_y + (screen_h / 2);
	int old_x = screen_cx;
	int old_y = screen_cy;
	int ratio_x = screen_w / 255;
	int ratio_y = screen_h / 255;
	int mouse_rate = 5;
	//int mouse_sensitivity = 10;
	int mouse_sensitivity = 10000;
	int pos_sensitivity = 30;

	// Blur
	const int BLUR = 0;
	// Puzzle Quest 2
	const int PQ2 = 1;
	// Half Life 2
	const int HL2 = 2;
	// Rayman Origins
	const int RAYO = 3;
	// Super Mario Crossover
	const int SMC = 4;
	// Shift
	const int SHIFT = 5;
	// Exit Path
	const int EPATH = 6;
	// Waves
	const int WAVES = 7;
	// Ys Origin
	const int YSO = 8;
	// Continuity
	const int CONT = 9;

	int mode = YSO;




	// Do we convert joystick control to mouse input?
	// Boolean
	int joyMouse = 0;
	int clickButton = 0;
	//int rightClickButton = 0;

	const int joyL1 = 0;
	const int joyL2 = 1;
	const int joyL3 = 2;
	const int joyR1 = 3;
	const int joyR2 = 4;
	const int joyR3 = 5;
	const int joyUp = 6;
	const int joyDown = 7;
	const int joyLeft = 8;
	const int joyRight = 9;
	const int joySqr = 10;
	const int joyTri = 11;
	const int joyCir = 12;
	const int joyX = 13;
	const int joyLL = 14;
	const int joyLU = 15;
	const int joyLR = 16;
	const int joyLD = 17;
	const int joyRL = 18;
	const int joyRU = 19;
	const int joyRR = 20;
	const int joyRD = 21;

	const int numButtons = 22;
	const int joyPos = 14;
	// Boolean
	int pressed[14] = {0,0,0,0,0,0,0,0,0,0,0,0,0,0};
	BYTE joy2key[22] = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};


	const BYTE kbUp = 0x52;
	const BYTE kbDown = 0x51;
	const BYTE kbLeft = 0x50;
	const BYTE kbRight = 0x4F;
	const BYTE kbX = 0x1B;
	const BYTE kbEnter = 0x28;
	const BYTE kbSpace = 0x2C;
	const BYTE kbRCtrl = 0xE4;
	const BYTE kbS = 0x16;
	const BYTE kbA = 0x04;
	const BYTE kbQ = 0x14;
	const BYTE kbW = 0x1A;
	const BYTE kbEsc = 0x29;
	const BYTE kbZ = 0x1D;
	const BYTE kbD = 0x07;
	const BYTE kbC = 0x06;
	// Left shift
	const BYTE kbShift = 0xE1;

	SOCKET ls = INVALID_SOCKET;
	SOCKET client;

	if(mode == BLUR) {
		joyMouse = 0;

		joy2key[joyL1] = kbUp;
		joy2key[joyL2] = kbDown;
		joy2key[joyR1] = kbW;
		joy2key[joyR2] = kbS;
		joy2key[joyUp] = kbUp;
		joy2key[joyDown] = kbDown;
		joy2key[joyLeft] = kbLeft;
		joy2key[joyRight] = kbRight;
		joy2key[joySqr] = kbEnter;
		joy2key[joyTri] = kbEsc;
		joy2key[joyCir] = kbA;
		joy2key[joyX] = kbQ;
		joy2key[joyLL] = kbLeft;
		joy2key[joyLR] = kbRight;

	} else if(mode == PQ2) {
		joyMouse = 1;
		clickButton = joyX;

	} else if(mode == HL2) {
		joyMouse = 1;
		clickButton = joyX;
	} else if(mode == RAYO) {
		joyMouse = 0;

		joy2key[joyUp] = kbUp;
		joy2key[joyDown] = kbDown;
		joy2key[joyLeft] = kbLeft;
		joy2key[joyRight] = kbRight;
		joy2key[joyX] = kbSpace;
		joy2key[joySqr] = kbS;
		joy2key[joyTri] = kbEsc;
		joy2key[joyLU] = kbUp;
		joy2key[joyLD] = kbDown;
		joy2key[joyLL] = kbLeft;
		joy2key[joyLR] = kbRight;
	} else if(mode == SMC) {
		joyMouse = 0;

		joy2key[joyUp] = kbUp;
		joy2key[joyDown] = kbDown;
		joy2key[joyLeft] = kbLeft;
		joy2key[joyRight] = kbRight;
		joy2key[joyLU] = kbUp;
		joy2key[joyLD] = kbDown;
		joy2key[joyLL] = kbLeft;
		joy2key[joyLR] = kbRight;
		joy2key[joyX] = kbZ;
		joy2key[joyR1] = kbX;
		joy2key[joyR2] = kbS;
	} else if(mode == SHIFT) {
		joyMouse = 0;

		joy2key[joyLeft] = kbLeft;
		joy2key[joyRight] = kbRight;
		joy2key[joyLL] = kbLeft;
		joy2key[joyLR] = kbRight;
		joy2key[joyX] = kbSpace;
		joy2key[joyR1] = kbShift;
	} else if(mode == EPATH) {
		joyMouse = 0;

		joy2key[joyDown] = kbDown;
		joy2key[joyLeft] = kbLeft;
		joy2key[joyRight] = kbRight;
		joy2key[joyLD] = kbDown;
		joy2key[joyLL] = kbLeft;
		joy2key[joyLR] = kbRight;
		joy2key[joyX] = kbUp;
		joy2key[joyR1] = kbSpace;
	} else if(mode == WAVES) {
		joyMouse = 0;
		clickButton = joyL1;
		//rightClickButton = joyR1;

		joy2key[joyL1] = kbQ;
		joy2key[joyR1] = kbSpace;
		joy2key[joyTri] = kbEsc;

		joy2key[joyLU] = kbW;
		joy2key[joyLD] = kbS;
		joy2key[joyLL] = kbA;
		joy2key[joyLR] = kbD;
		joy2key[joyRU] = kbUp;
		joy2key[joyRD] = kbDown;
		joy2key[joyRL] = kbLeft;
		joy2key[joyRR] = kbRight;
	} else if(mode == YSO) {
		joyMouse = 0;

		// Jump
		joy2key[joyX] = kbX;
		// Attack
		joy2key[joySqr] = kbZ;
		// Change weapon
		joy2key[joyCir] = kbD;
		// Charge weapon
		joy2key[joyR1] = kbC;
		// Pause
		joy2key[joyTri] = kbEsc;

		joy2key[joyUp] = kbUp;
		joy2key[joyDown] = kbDown;
		joy2key[joyLeft] = kbLeft;
		joy2key[joyRight] = kbRight;

		joy2key[joyLU] = kbUp;
		joy2key[joyLD] = kbDown;
		joy2key[joyLL] = kbLeft;
		joy2key[joyLR] = kbRight;
	} else if(mode == CONT) {
		joyMouse = 0;

		// Zoom
		joy2key[joyR1] = kbSpace;
		// Jump
		joy2key[joyX] = kbUp;

		// Move (blocks)
		joy2key[joyUp] = kbUp;
		joy2key[joyDown] = kbDown;
		joy2key[joyLeft] = kbLeft;
		joy2key[joyRight] = kbRight;
	} else {
		printf("Not a valid button configuration!");
	}

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
		int iKey = 0;
		BYTE mouse_buttons = 0;
		iResult = recv(client, recvbuf, recvbuflen, 0);
		if (iResult > 0) {
			USHORT buttons = 0;
            BYTE hat = 0, x = 0, y = 128, rx = 128, ry = 64, throttle = 0;
			//printf("Bytes received: %d\n", iResult);

			//START Anna
			//
            // Send the keyboard report
            //

            // See http://www.usb.org/developers/devclass_docs/Hut1_11.pdf (P53)
            // for a list of key codes            
            
            BYTE shiftKeys = KBD_LGUI_BIT;
            BYTE keyCodes[KBD_KEY_CODES] = {0, 0, 0, 0, 0, 0};
            //printf("Sending keyboard report\n");
			//END Anna

			// do stuff
			
			if(iResult < 12){
				printf("Expected at least 12 bytes.\n");
				exit(-1);
			}

			/*for(i = 0; i < 12; i++){
				printf("%u ", (unsigned char) recvbuf[i]);
			}
			printf("---\n");*/


			x = (BYTE)((((unsigned int) recvbuf[1]) + 128) % 256); // (BYTE) recvbuf[1];
			y = (BYTE)((((unsigned int) recvbuf[3]) + 128) % 256);// (BYTE) recvbuf[3];
			rx = (BYTE) recvbuf[5];
			ry = (BYTE) recvbuf[7];

			new_pos_x = ((int) (unsigned char) recvbuf[1]) - 128;
			new_pos_y = ((int) (unsigned char) recvbuf[3]) - 128;
			new_pos_rx = ((int) (unsigned char) recvbuf[5]) - 128;
			new_pos_ry = ((int) (unsigned char) recvbuf[7]) - 128;

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

			pressed[joyL1] = (recvbuf[8] & 128) && 1;
			pressed[joyL2] = (recvbuf[8] & 64) && 1;
			pressed[joyL3] = (recvbuf[8] & 32) && 1;
			pressed[joyR1] = (recvbuf[8] & 16) && 1;
			pressed[joyR2] = (recvbuf[8] & 8) && 1;
			pressed[joyR3] = (recvbuf[8] & 4) && 1;
			pressed[joyUp] = (recvbuf[8] & 2) && 1;
			pressed[joyDown] = (recvbuf[8] & 1) && 1;
			pressed[joyLeft] =(recvbuf[9] & 128) && 1;
			pressed[joyRight] = (recvbuf[9] & 64) && 1;
			pressed[joySqr] = (recvbuf[9] & 32) && 1;
			pressed[joyTri] = (recvbuf[9] & 16) && 1;
			pressed[joyCir] = (recvbuf[9] & 8) && 1;
			pressed[joyX] = (recvbuf[9] & 4) && 1;

			//
            // Send the joystick report
            //
            
            //printf("Sending joystick report\n");

            vmulti_update_joystick(vmulti, buttons, hat, x, y, rx, ry, throttle);

			// Anna
            //shiftKeys = KBD_LSHIFT_BIT;
			shiftKeys = 0;
			iKey = 0;
			
			//if(kb_cnt >= kb_cntmax) {
				for(i = 0; i < joyPos; i++) {
					if(pressed[i] && joy2key[i] > 0) {
						//printf("-----------------Pressed %d",i);
						keyCodes[iKey] = joy2key[i];
						iKey++;
					}
				}
			//	kb_cnt = 0;
			//} else {
			//	kb_cnt++;
			//}

			
			if(joy2key[joyLU] > 0 && new_pos_y < -pos_sensitivity) {
				keyCodes[iKey] = joy2key[joyLU];
				iKey++;
			} else if(joy2key[joyLD] > 0 && new_pos_y > pos_sensitivity) {
				keyCodes[iKey] = joy2key[joyLD];
				iKey++;
			}
			if(joy2key[joyLL] > 0 && new_pos_x < -pos_sensitivity) {
				//printf("--------------left------ %d    %d\n",x,-pos_sensitivity);
				keyCodes[iKey] = joy2key[joyLL];
				iKey++;
			} else if(joy2key[joyLR] > 0 && new_pos_x > pos_sensitivity) {
				//printf("--------------right----- %d    %d\n",x,pos_sensitivity);
				keyCodes[iKey] = joy2key[joyLR];
				iKey++;
			}
			if(joy2key[joyRU] > 0 && new_pos_ry < -pos_sensitivity) {
				keyCodes[iKey] = joy2key[joyRU];
				iKey++;
			} else if(joy2key[joyRD] > 0 && new_pos_ry > pos_sensitivity) {
				keyCodes[iKey] = joy2key[joyRD];
				iKey++;
			}
			if(joy2key[joyRL] > 0 && new_pos_rx < -pos_sensitivity) {
				keyCodes[iKey] = joy2key[joyRL];
				iKey++;
			} else if(joy2key[joyRR] > 0 && new_pos_rx > pos_sensitivity) {
				keyCodes[iKey] = joy2key[joyRR];
				iKey++;
			}

			/*for(i = joyPos-1; i < numButtons; i++) {
				if(joy2key[i] > 0) {
					if(new_pos_x > -pos_sensitivity && new_pos_x < pos_sensitivity) {
						mouse_x = old_x;
					}
					if(new_pos_y > -pos_sensitivity && new_pos_y < pos_sensitivity) {
						mouse_y = old_y;
					}
					printf("-----------------Pressed %d",i);
					keyCodes[iKey] = joy2key[i];
					iKey++;
				}
			}*/

			vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);

			//This first "if" is for testing only
			if(!pressed[joyTri]) {
				if(joyMouse) {
					//printf("-------------Move Mouse--------------");
					if(pressed[clickButton]) {
						mouse_buttons = 1; // left click
					}
					/*if(pressed[rightClickButton]) {
						mouse_buttons = 2; // Something
					}*/

					if(mouse_cnt == mouse_cntmax) {
						mouse_cnt = 0;
						//new_pos_x = ((int) (unsigned char) recvbuf[1]) - 128;
						//new_pos_y = ((int) (unsigned char) recvbuf[3]) - 128;

						// Right
						if(new_pos_x > old_pos_x){
							// old and new both left of center
							if(new_pos_x <= 0) {
								mouse_x = old_x;
							// old and new both right of center
							} else if(old_pos_x >= 0) {
								mouse_x = old_x + mouse_rate*new_pos_x;
							// old left of center, new right of center
							} else {
								mouse_x = old_x + mouse_rate*new_pos_x;
							}
						// Center X
						// old and new in same position
						} else if(new_pos_x == old_pos_x) {
							mouse_x = old_x + mouse_rate*new_pos_x;
						// Left
						} else {
							// old and new both left of center
							if(old_pos_x <= 0) {
								mouse_x = old_x + mouse_rate*new_pos_x;
							// old and new both right of center
							} else if(new_pos_x >= 0) {
								mouse_x = old_x;
							// old right of center, new left of center
							} else {
								mouse_x = old_x + mouse_rate*new_pos_x;
							}
						}

						// Down
						if(new_pos_y > old_pos_y){
							// old and new both above center
							if(new_pos_y <= 0) {
								mouse_y = old_y;
							// old and new both below center
							} else if(old_pos_y >= 0) {
								mouse_y = old_y + mouse_rate*new_pos_y;
							// old above center, new below center
							} else {
								mouse_y = old_y + mouse_rate*new_pos_y;
							}
						// Center Y
						// old and new in same position
						} else if(new_pos_y == old_pos_y) {
							mouse_y = old_y + mouse_rate*new_pos_y;
						// Up
						} else {
							// old and new both above center
							if(old_pos_y <= 0) {
								mouse_y = old_y + mouse_rate*new_pos_y;
							// old and new both below center
							} else if(new_pos_y >= 0) {
								mouse_y = old_y;
							// old below center, new above center
							} else {
								mouse_y = old_y + mouse_rate*new_pos_y;
							}
						}
					
						// Don't be too senstive

						if(new_pos_x > -pos_sensitivity && new_pos_x < pos_sensitivity) {
							mouse_x = old_x;
						}
						if(new_pos_y > -pos_sensitivity && new_pos_y < pos_sensitivity) {
							mouse_y = old_y;
						}
						if(mouse_y < 0){
							mouse_y = 0;
						} else if(mouse_y > screen_h) {
							mouse_y = screen_h;
						}
						if(mouse_x < 0) {
							mouse_x = 0;
						} else if(mouse_x > screen_w) {
							mouse_x = screen_w;
						}

						old_x = mouse_x;
						old_y = mouse_y;
						old_pos_x = new_pos_x;
						old_pos_y = new_pos_y;

						vmulti_update_mouse(vmulti, mouse_buttons, mouse_x, mouse_y, 0);
					} else {
						mouse_cnt++;
					}
				}
			}


			/*
			// x
			if((recvbuf[9] & 4) && 1) {
				//keyCodes[iKey] = 0x58; // enter
				keyCodes[iKey] = 0x52; // uparrow
				mouse_buttons = 1; // left click
				iKey++;
				//keyCodes[0] = 0x1B; // x
			}
			//circle
			if((recvbuf[9] & 8) && 1) {

			}
			//up
			if((recvbuf[8] & 2) && 1) {
				keyCodes[iKey] = 0x52; // uparrow
				iKey++;
			}
			//down
			if((recvbuf[8] & 1) && 1) {
				keyCodes[iKey] = 0x51; // downarrow
				iKey++;
			}
			//left
			if((recvbuf[9] & 128) && 1) {
				keyCodes[iKey] = 0x50; // leftarrow
				iKey++;
			}
			//right
			if((recvbuf[9] & 64) && 1) {
				keyCodes[iKey] = 0x4F; // rightarrow
				iKey++;
			}
			vmulti_update_keyboard(vmulti, shiftKeys, keyCodes);
			*/
			// tri
			/*if(!((recvbuf[9] & 16) && 1)) {
				if(mouse_cnt == mouse_cntmax) {
					mouse_cnt = 0;
					//printf("oldposx: %d    ",old_pos_x);
					new_pos_x = ((int) (unsigned char) recvbuf[1]) - 128;
					new_pos_y = ((int) (unsigned char) recvbuf[3]) - 128;
					//printf("oldposx: %d    newposx: %d    \n",old_pos_x,new_pos_x);
					//printf("oldposy: %d    newposy: %d    \n",old_pos_y,new_pos_y);
					// Right
					if(new_pos_x > old_pos_x){
						// old and new both left of center
						if(new_pos_x <= 0) {
							//printf("2leftR    ");
							mouse_x = old_x;
						// old and new both right of center
						} else if(old_pos_x >= 0) {
							//printf("2rightR    ");
							mouse_x = old_x + mouse_rate*new_pos_x;
						// old left of center, new right of center
						} else {
							//printf("left2rightR    ");
							mouse_x = old_x + mouse_rate*new_pos_x;
						}
					// Center X
					// old and new in same position
					} else if(new_pos_x == old_pos_x) {
						//printf("centerx    ");
						mouse_x = old_x + mouse_rate*new_pos_x;
						//printf("centerx: %d    ",mouse_x);
					// Left
					} else {
						// old and new both left of center
						if(old_pos_x <= 0) {
							//printf("2leftL    ");
							mouse_x = old_x + mouse_rate*new_pos_x;
						// old and new both right of center
						} else if(new_pos_x >= 0) {
							//printf("2rightL    ");
							mouse_x = old_x;
						// old right of center, new left of center
						} else {
							//printf("right2leftL    ");
							mouse_x = old_x + mouse_rate*new_pos_x;
						}
					}

					// Down
					if(new_pos_y > old_pos_y){
						// old and new both above center
						if(new_pos_y <= 0) {
							//printf("2aboveD    ");
							mouse_y = old_y;
						// old and new both below center
						} else if(old_pos_y >= 0) {
							//printf("2belowD    ");
							mouse_y = old_y + mouse_rate*new_pos_y;
						// old above center, new below center
						} else {
							//printf("above2belowD    ");
							mouse_y = old_y + mouse_rate*new_pos_y;
						}
					// Center Y
					// old and new in same position
					} else if(new_pos_y == old_pos_y) {
						//printf("centery    ");
						mouse_y = old_y + mouse_rate*new_pos_y;
					// Up
					} else {
						// old and new both above center
						if(old_pos_y <= 0) {
							//printf("2aboveU    ");
							mouse_y = old_y + mouse_rate*new_pos_y;
						// old and new both below center
						} else if(new_pos_y >= 0) {
							//printf("2belowU    ");
							mouse_y = old_y;
						// old below center, new above center
						} else {
							//printf("below2aboveU    ");
							mouse_y = old_y + mouse_rate*new_pos_y;
						}
					}
					
					// Don't be too senstive
					/*if(old_pos_x - new_pos_x > -mouse_sensitivity && old_pos_x - new_pos_x < mouse_sensitivity) {
						mouse_x = old_x;
					}
					if(old_y - mouse_y > -mouse_sensitivity && old_y - mouse_y < mouse_sensitivity) {
						mouse_y = old_y;
					} 
					if(new_pos_x > -pos_sensitivity && new_pos_x < pos_sensitivity) {
						mouse_x = old_x;
					}
					if(new_pos_y > -pos_sensitivity && new_pos_y < pos_sensitivity) {
						mouse_y = old_y;
					}
					/*if(mouse_y < 0){
						mouse_y = 0;
					} else if(mouse_y > screen_h) {
						mouse_y = screen_h;
					}
					if(mouse_x < 0) {
						mouse_x = 0;
					} else if(mouse_x > screen_w) {
						mouse_x = screen_w;
					}

					//printf("oldx: %d    newx: %d    oldy: %d    newy: %d\n",old_x,mouse_x,old_y,mouse_y);

					old_x = mouse_x;
					old_y = mouse_y;
					old_pos_x = new_pos_x;
					old_pos_y = new_pos_y;

					//printf("x: %d y: %d\n",mouse_x,mouse_y);
					vmulti_update_mouse(vmulti, mouse_buttons, mouse_x, mouse_y, 0);
				} else {
					mouse_cnt++;
				}
			}*/

			// tri
			/*if(!((recvbuf[9] & 16) && 1)) {
				if(mouse_cnt == mouse_cntmax) {
					mouse_cnt = 0;
					mouse_x = screen_cx + (((int) (unsigned char) recvbuf[1]) - 128)*ratio_x;
					mouse_y = screen_cy + (((int) (unsigned char) recvbuf[3]) - 128)*ratio_y;
					//mouse_x = old_x + (((int) (unsigned char) recvbuf[1]) - 128)*ratio_x;
					//mouse_y = old_y + (((int) (unsigned char) recvbuf[3]) - 128)*ratio_y;
					if(old_x - mouse_x < -mouse_sensitivity || old_x - mouse_x > mouse_sensitivity) {
						//printf("old: %d    new: %d    diff: %d\n",old_x,mouse_x,(old_x-mouse_x));
						old_x = mouse_x;
					} else {
						mouse_x = old_x;
					}
					if(old_y - mouse_y < -mouse_sensitivity || old_y - mouse_y > mouse_sensitivity) {
						old_y = mouse_y;
					} else {
						mouse_y = mouse_y;
					}
					printf("x: %d y: %d\n",mouse_x,mouse_y);
					vmulti_update_mouse(vmulti, mouse_buttons, mouse_x, mouse_y, 0);
				} else {
					mouse_cnt++;
				}
			}*/

			//printf("Sending mouse report\n");
            //vmulti_update_mouse(vmulti, 0, 0, 0, 0);
			//Sleep(100);
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
