#include "stdafx.h"
#include "winerror.h"
#include "Winsock2.h"
#pragma comment(lib, "ws2_32")

#include "windows.h"


#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;


/// �궨��
#define PORT 5050
#define DATA_BUFSIZE 8192

#define OutErr(a) cout << (a) << endl;  \
cout<< "������룺" << WSAGetLastError() << endl; \
cout<< "�����ļ���" << __FILE__ << endl;  \
cout<< "����������" << __LINE__ << endl;  

#define OutMsg(a) cout << (a) << endl;

/// ȫ�ֺ�������


///////////////////////////////////////////////////////////////////////
//
// ������       : InitWinsock
// ��������     : ��ʼ��WINSOCK
// ����ֵ       : void 
//
///////////////////////////////////////////////////////////////////////
void InitWinsock()
{
    // ��ʼ��WINSOCK
    WSADATA wsd;
    if( WSAStartup(MAKEWORD(2, 2), &wsd) != 0)
	{
		OutErr("iniSocket error");
	}
        
}

///////////////////////////////////////////////////////////////////////
//
// ������       : BindServerOverlapped
// ��������     : �󶨶˿ڣ�������һ�� Overlapped ��Listen Socket
// ����         : int nPort
// ����ֵ       : SOCKET 
//
///////////////////////////////////////////////////////////////////////
SOCKET BindServerOverlapped(int nPort)
{
    // ����socket 
    SOCKET sServer = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED);
    
    // �󶨶˿�
    struct sockaddr_in servAddr;
    servAddr.sin_family = AF_INET;
    servAddr.sin_port = htons(nPort);
    servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
    
    if(bind(sServer, (struct sockaddr *)&servAddr, sizeof(servAddr)) < 0)
    {
        OutErr("bind Failed!");
        return NULL;
    }
    
    // ���ü�������Ϊ200
    if(listen(sServer, 200) != 0)
    {
        OutErr("listen Failed!");
        return NULL;
    }
    return sServer;
}


/// �ṹ�嶨�壨ֻ��Ҫ��һ����Ա��overlapped��
typedef struct
{
    OVERLAPPED Overlapped;
    WSABUF DataBuf;
    CHAR Buffer[DATA_BUFSIZE];
} PER_IO_OPERATION_DATA, * LPPER_IO_OPERATION_DATA;


typedef struct 
{
    SOCKET Socket;
} PER_HANDLE_DATA, * LPPER_HANDLE_DATA;

vector<LPPER_HANDLE_DATA> _Client;
DWORD start;

DWORD WINAPI ProcessIO(LPVOID lpParam)
{
    HANDLE CompletionPort = (HANDLE)lpParam;
    DWORD BytesTransferred;
    LPPER_HANDLE_DATA PerHandleData;
    LPPER_IO_OPERATION_DATA PerIoData;
    
    while(true)
    {
        
        if(0 == GetQueuedCompletionStatus(CompletionPort, &BytesTransferred, (LPDWORD)&PerHandleData, (LPOVERLAPPED*)&PerIoData, INFINITE))
        {
            if( (GetLastError() == WAIT_TIMEOUT) || (GetLastError() == ERROR_NETNAME_DELETED) )
            {
                cout << "closing socket" << PerHandleData->Socket << endl;

                closesocket(PerHandleData->Socket);
				//vector<LPPER_HANDLE_DATA>::iterator iter =  find(_Client.begin(),_Client.end(),*PerHandleData);
				vector<LPPER_HANDLE_DATA>::iterator iter =_Client.begin();
				for(;iter!=_Client.end();iter++)
				{
					if((*iter)->Socket == PerHandleData->Socket)
					{
						_Client.erase(iter);
					}
				}


                delete PerIoData;
                delete PerHandleData;
                continue;
            }
            else
            {
                OutErr("GetQueuedCompletionStatus failed!");
            }
            return 0;
        }
        
        // ˵���ͻ����Ѿ��˳�
        if(BytesTransferred == 0)
        {
            cout << "closing socket" << PerHandleData->Socket << endl;
            
			vector<LPPER_HANDLE_DATA>::iterator iter =_Client.begin();
			for(;iter!=_Client.end();iter++)
			{
				if((*iter)->Socket == PerHandleData->Socket)
				{
					_Client.erase(iter);
				}
			}
			closesocket(PerHandleData->Socket);
            delete PerIoData;
            delete PerHandleData;
            continue;
        }
        
        // ȡ�����ݲ�����
        //cout << PerHandleData->Socket << "���͹�������Ϣ��" << PerIoData->Buffer << endl;
        
        // ������ socket Ͷ��WSARecv����
        DWORD Flags = 0;
        DWORD dwRecv = 0;
        ZeroMemory(PerIoData, sizeof(PER_IO_OPERATION_DATA));
        PerIoData->DataBuf.buf = PerIoData->Buffer;
        PerIoData->DataBuf.len = DATA_BUFSIZE;
        WSARecv(PerHandleData->Socket, &PerIoData->DataBuf	, 1, &dwRecv, &Flags, &PerIoData->Overlapped, NULL); 
    }
    
    return 0;
}

void borcast()
{

	vector<LPPER_HANDLE_DATA>::iterator iter =_Client.begin();

	GetTickCount();
	for(;iter != _Client.end();iter++)
	{
		SOCKET sClient=(*iter)->Socket;
		// ����һ��Overlapped����ʹ�����Overlapped�ṹ��socketͶ�ݲ���
		LPPER_IO_OPERATION_DATA PerIoData = new PER_IO_OPERATION_DATA();

		ZeroMemory(PerIoData, sizeof(PER_IO_OPERATION_DATA));
		PerIoData->DataBuf.buf = PerIoData->Buffer;
		PerIoData->DataBuf.len = DATA_BUFSIZE;
		PerIoData->Buffer[0]='h';
		// Ͷ��һ��WSARecv����
		DWORD Flags = 0;
		DWORD dwRecv = 0;
		WSASend(sClient, &PerIoData->DataBuf, 1, &dwRecv, Flags, &PerIoData->Overlapped, NULL);
	}
}

void main()
{
    InitWinsock();
    
    HANDLE CompletionPort = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);
    
    // ����ϵͳ��CPU�������������߳�
    SYSTEM_INFO SystemInfo;
    GetSystemInfo(&SystemInfo);
    
    for(int i = 0; i < SystemInfo.dwNumberOfProcessors * 2; i++)
    {
        HANDLE hProcessIO = CreateThread(NULL, 0, ProcessIO, CompletionPort, 0, NULL);
            
    }
    
    // ��������SOCKET
    SOCKET sListen = BindServerOverlapped(PORT);
    
    
    SOCKET sClient;
    LPPER_HANDLE_DATA PerHandleData;
    LPPER_IO_OPERATION_DATA PerIoData;
    while(true)
    {
        // �ȴ��ͻ��˽���
        //sClient = WSAAccept(sListen, NULL, NULL, NULL, 0);
        sClient = accept(sListen, 0, 0);
        
        cout << "Socket " << sClient << "���ӽ���" << endl;
        
        PerHandleData = new PER_HANDLE_DATA();
        PerHandleData->Socket = sClient;
        
        // �����ӽ�����Socket
        CreateIoCompletionPort((HANDLE)sClient, CompletionPort, (DWORD)PerHandleData, 0);
        
        // ����һ��Overlapped����ʹ�����Overlapped�ṹ��socketͶ�ݲ���
        PerIoData = new PER_IO_OPERATION_DATA();

        ZeroMemory(PerIoData, sizeof(PER_IO_OPERATION_DATA));
        PerIoData->DataBuf.buf = PerIoData->Buffer;
        PerIoData->DataBuf.len = DATA_BUFSIZE;
        
        // Ͷ��һ��WSARecv����
        DWORD Flags = 0;//����0 ������
        DWORD dwRecv = 0;//���ܵ����ֽ�
        WSARecv(sClient, &PerIoData->DataBuf, 1, &dwRecv, &Flags, &PerIoData->Overlapped, NULL);
		_Client.push_back(PerHandleData);
		borcast();		
    }
    
    DWORD dwByteTrans;
    PostQueuedCompletionStatus(CompletionPort, dwByteTrans, 0, 0);
    closesocket(sListen);
}