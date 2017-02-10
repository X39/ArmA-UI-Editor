#pragma once
#include "Base.h"
namespace sqf
{
	class String : public Base
	{
	private:
		std::string _value;
	public:
		String::String(std::string Value = "") : _value(Value) {};
		String::~String() {};

		inline Type String::getType(void) const
		{
			return Type::STRING;
		}
		inline std::string String::escapedString(void) const
		{
			return std::string("\"").append(String::escapeString(this->_value)).append("\"");
		}
		
		inline const std::string String::getValue(void) const
		{
			return this->_value;
		}
		inline std::string String::getValue(void)
		{
			return this->_value;
		}
		inline void String::setValue(std::string val)
		{
			this->_value = val;
		}
		static std::string String::escapeString(const std::string s)
		{
			std::string returnString;
			for (auto& c : s)
			{
				switch (c)
				{
				default:
					returnString.append(&c, &c + 1);
					break;
				case '"':
					returnString.append("\"\"");
					break;
				case '\n':
					returnString.append("\\n");
					break;
				case '\t':
					returnString.append("\\t");
					break;
				case '\r':
					returnString.append("\\r");
					break;
				}
			}
			return returnString;
		}
		static std::string String::unescapeString(const std::string s)
		{
			std::string returnString;
			unescapeString(s, &returnString);
			return returnString;
		}
		static size_t String::unescapeString(const std::string s, std::string* returnString)
		{
			bool flag = false;
			for (size_t i = 0; i < s.length(); i++)
			{
				auto& c = s[i];
				switch (c)
				{
				case '\\':
					i++;
					switch (*(&c + 1))
					{
					case 't':
						returnString->append("\t");
						break;
					case 'n':
						returnString->append("\n");
						break;
					case 'r':
						returnString->append("\r");
						break;
					}
					break;
				case '"':
					if (*(&c + 1) == '"')
						i++;
					else
						return i;
				default:
					returnString->append(&c, &c + 1);
					break;
				}
			}
			return s.length();
		}
		static unsigned int String::parsePartially(String* out, std::string in)
		{
			if (in[0] != '"')
				throw std::exception("String has to start with \" to be parsed");
			std::string s;
			size_t len = String::unescapeString(in.substr(1), &s);
			out->setValue(s);
			return len + 1;
		}
	};
}