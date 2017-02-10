#pragma once
#include "Base.h"
namespace sqf
{
	class Nil : public Base
	{
	public:
		Nil::Nil() {};
		Nil::~Nil() {};

		inline Type Nil::getType(void) const
		{
			return Type::BOOLEAN;
		}
		inline std::string Nil::escapedString(void) const
		{
			return "NIL";
		}
	};
}