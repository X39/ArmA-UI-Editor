#ifdef _DEBUG
#include "dllMain.h"

// Standad C++ includes
#include <iostream>
#include <cstdlib>
#include <string>
#include <ctime>
#include <Windows.h>
#include "sqf/Array.h"
using namespace std;

int main(int argc, char* args[])
{
	time_t t = time(NULL);
	char output[8192];
	cout << "====INITIALIZE DLL====" << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("INIT").append("\"]").c_str()); cout << output << endl;

	cout << "====BASE TESTS====" << endl;
	////Base Tests
	RVExtension(output, 8192, std::string().append("[\"").append("VERSION").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("INITIALIZED").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("OPEN").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("CLOSE").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("OPEN").append("\",").append("0").append("]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("CLOSE").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("OPEN").append("\",").append("5").append("]").c_str()); cout << output << endl;
	cout << "Sleeping for 3s" << endl;
	Sleep(3 * 1000);
	RVExtension(output, 8192, std::string().append("[\"").append("KEEPOPEN").append("\",\"").append("").append("\"]").c_str()); cout << output << endl;
	cout << "Sleeping for 6s" << endl;
	Sleep(6 * 1000);
	RVExtension(output, 8192, std::string().append("[\"").append("KEEPOPEN").append("\",\"").append("").append("\"]").c_str()); cout << output << endl;

	cout << "====SQL TESTS====" << endl;
	//SQL Tests
	RVExtension(output, 8192, std::string().append("[\"").append("OPEN").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("CREATESTMT").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("QUERY").append("\",0,\"").append("SELECT * FROM `test`").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("NEXT").append("\",0]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("NEXT").append("\",0]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("NEXT").append("\",0]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("NEXT").append("\",0]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("CLOSE").append("\",0]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("GETLASTERROR").append("\"]").c_str()); cout << output << endl;

	cout << "====ERROR TESTS - INVALID INPUT====" << endl;
	//Error Tests - Invalid Input
	RVExtension(output, 8192, ""); cout << "Test  1 result:" << output << endl;
	RVExtension(output, 8192, "test"); cout << "Test  2 result:" << output << endl;
	RVExtension(output, 8192, "1"); cout << "Test  3 result:" << output << endl;
	RVExtension(output, 8192, "12"); cout << "Test  4 result:" << output << endl;
	RVExtension(output, 8192, "123"); cout << "Test  5 result:" << output << endl;
	RVExtension(output, 8192, "1234"); cout << "Test  6 result:" << output << endl;
	RVExtension(output, 8192, "]VERSION, BLA["); cout << "Test  7 result:" << output << endl;
	RVExtension(output, 8192, "[VERSION"); cout << "Test  8 result:" << output << endl;
	RVExtension(output, 8192, "[\"VERSION\""); cout << "Test  9 result:" << output << endl;
	RVExtension(output, 8192, "[VERSION, FLASE]"); cout << "Test 10 result:" << output << endl;
	RVExtension(output, 8192, "BLA[BLA]BLA["); cout << "Test 11 result:" << output << endl;
	RVExtension(output, 8192, ","); cout << "Test 12 result:" << output << endl;
	RVExtension(output, 8192, "[,]"); cout << "Test 13 result:" << output << endl;
	RVExtension(output, 8192, "[ , ]"); cout << "Test 14 result:" << output << endl;
	RVExtension(output, 8192, "[\", \"]"); cout << "Test 15 result:" << output << endl;
	RVExtension(output, 8192, "[,,,,,]"); cout << "Test 16 result:" << output << endl;
	RVExtension(output, 8192, "]]]]]"); cout << "Test 17 result:" << output << endl;
	RVExtension(output, 8192, "[][][][][]"); cout << "Test 18 result:" << output << endl;
	RVExtension(output, 8192, "12265944qwez"); cout << "Test 19 result:" << output << endl;
	RVExtension(output, 8192, "1[2265,944]qwez"); cout << "Test 20 result:" << output << endl;
	RVExtension(output, 8192, "1226]5944qw[ez,"); cout << "Test 21 result:" << output << endl;


	cout << "====ERROR TESTS - INVALID USE====" << endl;
	//Error Tests - Invalid use
	RVExtension(output, 8192, std::string().append("[\"").append("QUERY").append("\",\"").append("SELECT * FROM `test`").append("\", 0]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("QUERY").append("\",\"").append("SELECT * FROM `test`").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("QUERY").append("\",\"0\",\"").append("SELECT * FROM `test`").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, std::string().append("[\"").append("QUERY").append("\",\"1\",\"").append("SELECT * FROM `test`").append("\"]").c_str()); cout << output << endl;
	RVExtension(output, 8192, "[\"NEXT\",\"1\"]"); cout << output << endl;

	system("pause");
}
#endif