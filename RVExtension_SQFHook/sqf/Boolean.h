#pragma once
#include "Base.h"
namespace sqf
{
	class Boolean : public Base
	{
	private:
		bool _value;
	public:
		Boolean::Boolean(bool flag = false) : _value(flag) {};
		Boolean::~Boolean() {};

		inline Type Boolean::getType(void) const
		{
			return Type::BOOLEAN;
		}
		inline std::string Boolean::escapedString(void) const
		{
			return std::string((this->_value ? "true" : "false"));
		}
		inline void Boolean::setValue(bool flag)
		{
			this->_value = flag;
		}
		inline bool Boolean::getValue(void) const
		{
			return this->_value;
		}
		static unsigned int Boolean::parsePartially(Boolean* out, std::string in)
		{
			size_t len = 0;
			if (in[0] == 't')
			{
				len = strlen("true") - 1;
				out->setValue(true);
			}
			else if (in[0] == 'f')
			{
				len = strlen("false") - 1;
				out->setValue(false);
			}
			else
			{
				throw std::exception("Provided String contains no boolean");
			}
			return len;
		}
	};
}