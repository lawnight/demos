#include "stdafx.h"
#include "winerror.h"
#include "Winsock2.h"
#pragma comment(lib, "ws2_32")

#include "windows.h"


#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;


/// 宏定义
#define PORT 5050
#define DATA_BUFSIZE 8192

#define OutErr(a) cout << (a) << endl;  \
cout<< "出错代码：" << WSAGetLastError() << endl; \
cout<< "出错文件：" << __FILE__ << endl;  \
cout<< "出错行数：" << __LINE__ << endl;  

#define OutMsg(a) cout << (a) << endl;

/// 全局函数定义


///////////////////////////////////////////////////////////////////////
//
// 函数名       : InitWinsock
// 功能描述     : 初始化WINSOCK
// 返回值       : void 
//
///////////////////////////////////////////////////////////////////////
void InitWinsock()
{
    // 初始化WINSOCK
    WSADATA wsd;
    if( WSAStartup(MAKEWORD(2, 2), &wsd) != 0)
	{
		OutErr("iniSocket error");
	}
        
}

///////////////////////////////////////////////////////////////////////
//
// 函数名       : BindServerOverlapped
// 功能描述     : 绑定端口，并返回一个 Overlapped 的Listen Socket
// 参数         : int nPort
// 返回值       : SOCKET 
//
///////////////////////////////////////////////////////////////////////
SOCKET BindServerOverlapped(int nPort)
{
    // 创建socket 
    SOCKET sServer = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED);
    
    // 绑定端口
    struct sockaddr_in servAddr;
    servAddr.sin_family = AF_INET;
    servAddr.sin_port = htons(nPort);
    servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
    
    if(bind(sServer, (struct sockaddr *)&servAddr, sizeof(servAddr)) < 0)
    {
        OutErr("bind Failed!");
        return NULL;
    }
    
    // 设置监听队列为200
    if(listen(sServer, 200) != 0)
    {
        OutErr("listen Failed!");
        return NULL;
    }
    return sServer;
}


/// 结构体定义（只需要第一个成员是overlapped）
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
        
        // 说明客户端已经退出
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
        
        // 取得数据并处理
        //cout << PerHandleData->Socket << "发送过来的消息：" << PerIoData->Buffer << endl;
        
        // 继续向 socket 投递WSARecv操作
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
		// 建立一个Overlapped，并使用这个Overlapped结构对socket投递操作
		LPPER_IO_OPERATION_DATA PerIoData = new PER_IO_OPERATION_DATA();

		ZeroMemory(PerIoData, sizeof(PER_IO_OPERATION_DATA));
		PerIoData->DataBuf.buf = PerIoData->Buffer;
		PerIoData->DataBuf.len = DATA_BUFSIZE;
		PerIoData->Buffer[0]='h';
		// 投递一个WSARecv操作
		DWORD Flags = 0;
		DWORD dwRecv = 0;
		WSASend(sClient, &PerIoData->DataBuf, 1, &dwRecv, Flags, &PerIoData->Overlapped, NULL);
	}
}

void main()
{
    InitWinsock();
    
    HANDLE CompletionPort = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);
    
    // 根据系统的CPU来创建工作者线程
    SYSTEM_INFO SystemInfo;
    GetSystemInfo(&SystemInfo);
    
    for(int i = 0; i < SystemInfo.dwNumberOfProcessors * 2; i++)
    {
        HANDLE hProcessIO = CreateThread(NULL, 0, ProcessIO, CompletionPort, 0, NULL);
            
    }
    
    // 创建侦听SOCKET
    SOCKET sListen = BindServerOverlapped(PORT);
    
    
    SOCKET sClient;
    LPPER_HANDLE_DATA PerHandleData;
    LPPER_IO_OPERATION_DATA PerIoData;
    while(true)
    {
        // 等待客户端接入
        //sClient = WSAAccept(sListen, NULL, NULL, NULL, 0);
        sClient = accept(sListen, 0, 0);
        
        cout << "Socket " << sClient << "连接进来" << endl;
        
        PerHandleData = new PER_HANDLE_DATA();
        PerHandleData->Socket = sClient;
        
        // 绑定连接进来的Socket
        CreateIoCompletionPort((HANDLE)sClient, CompletionPort, (DWORD)PerHandleData, 0);
        
        // 建立一个Overlapped，并使用这个Overlapped结构对socket投递操作
        PerIoData = new PER_IO_OPERATION_DATA();

        ZeroMemory(PerIoData, sizeof(PER_IO_OPERATION_DATA));
        PerIoData->DataBuf.buf = PerIoData->Buffer;
        PerIoData->DataBuf.len = DATA_BUFSIZE;
        
        // 投递一个WSARecv操作
        DWORD Flags = 0;//设置0 就行了
        DWORD dwRecv = 0;//接受到的字节
        WSARecv(sClient, &PerIoData->DataBuf, 1, &dwRecv, &Flags, &PerIoData->Overlapped, NULL);
		_Client.push_back(PerHandleData);
		borcast();		
    }
    
    DWORD dwByteTrans;
    PostQueuedCompletionStatus(CompletionPort, dwByteTrans, 0, 0);
    closesocket(sListen);
}