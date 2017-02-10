#include <string>
extern "C"
{
	__declspec(dllexport) void __stdcall RVExtension(char *output, int outputSize, const char *function);
};
void toUpper(std::string& s);
void toUpper(char* s);
void addCommands(void);
std::string readConfig(void);
