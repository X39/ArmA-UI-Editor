#define _CRT_SECURE_NO_WARNINGS
#include <Windows.h>
extern "C"
{
	__declspec (dllexport) void __stdcall RVExtension(char *output, int outputSize, const char *function);
}

void __stdcall RVExtension(char *output, int outputSize, const char *function)
{
	//https://github.com/intercept/intercept/blob/4360bf568d401614b45fed477a97e858dbfaad7b/src/host/intercept_dll/intercept_dll.cpp#L80
	uintptr_t game_state_addr = (uintptr_t)*(uintptr_t*)((uintptr_t)output + outputSize + 8);
	strcpy(output, "foobartest");
}

BOOL WINAPI DllMain(_In_ HINSTANCE hinstDLL, _In_ DWORD fdwReason, _In_ LPVOID lpvReserved)
{
	switch (fdwReason)
	{
		case DLL_PROCESS_ATTACH:
		break;
		case DLL_PROCESS_DETACH:
		break;
		case DLL_THREAD_ATTACH:
		break;
		case DLL_THREAD_DETACH:
		break;
		default:

		break;
	}
}