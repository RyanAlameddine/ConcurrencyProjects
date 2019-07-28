#include <iostream>
#include <thread>
#include <Windows.h>

int main()
{
	auto ret = LoadLibrary(L"Z:\\Documents\\Visual Studio 2019\\Projects\\ConcurrencyCppConsole\\Debug\\Library.dll");
	std::cout << "Hello World!\n";
}