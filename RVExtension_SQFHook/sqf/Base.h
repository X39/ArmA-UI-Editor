#pragma once
#include <string>

namespace sqf
{
	enum Type
	{
		NA = -1,
		STRING = 0,
		SCALAR,
		BOOLEAN,
		ARRAY
	};
	class Base
	{
	public:
		virtual ~Base(void) {};
		virtual Type getType(void) const = 0;
		virtual std::string escapedString(void) const = 0;
	};
}