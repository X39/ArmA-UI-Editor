#include "dllMain.h"


#define WIN32_LEAN_AND_MEAN // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <intrin.h>

// Standad C++ includes
#include <vector>
using namespace std;




//Return array always: [<Success:BOOL>, <Error:STRING>, <FunctionResult:UNKNOWN>]
void __stdcall RVExtension(char *output, int outputSize, const char *function)
{
	//https://github.com/intercept/intercept/blob/4360bf568d401614b45fed477a97e858dbfaad7b/src/host/intercept_dll/intercept_dll.cpp#L80
	uintptr_t game_state_addr = (uintptr_t)*(uintptr_t*)((uintptr_t)output + outputSize + 8);
	strcpy(output, to_string(game_state_addr).c_str());
	__asm int 3 ;
}
int __stdcall RVExtensionArgs(char *output, int outputSize, const char *function, const char **args, int argCnt)
{
	strcpy(output, "HookEngine Initialized");
	__asm int 3;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}
