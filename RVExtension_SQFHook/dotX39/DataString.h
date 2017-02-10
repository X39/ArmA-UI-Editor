#pragma once
#include "Data.h"
namespace dotX39
{
	class DataString : public Data
	{
	private:
		std::string _data;
		std::string _name;
	public:
		DataString::DataString(std::string data = "", std::string name = "NOTSET", bool isEscaped = false) NOEXCEPT(false)
		{
			_name = name;
			if (isEscaped)
			{
				_data = "";
				for (const char* c = &(data[0]); c[0] != 0x00; c++)
				{
					if (c[0] == '\\')
					{
						c++;
						if (c[0] == 0x00)
							throw std::exception("Unexpected '\\0' after escape character '\\'");
						switch (c[0])
						{
							case 'n': _data.append("\n"); break;
							case 't': _data.append("\t"); break;
							case 'r': _data.append("\r"); break;
							case '\\':  case '"':  case '\'':  case '[':  case ']':
								_data.append(c, c + 1);
								break;
							default: throw std::exception(std::string("Unexpected '").append(c, c + 1).append("' after escape character '\\'").c_str());
						}
					}
					else
					{
						//ToDo: Remove next if after improoving array parser
						if (c[0] == ']' || c[0] == '[')
							throw std::exception("'[' and ']' have to be escaped in strings!");
						_data.append(c, c + 1);
					}
				}
			}
			else
			{
				_data = data;
			}
		}
		DataString::~DataString(void) NOEXCEPT(true)
		{
			
		}
		std::string DataString::getName(void) const NOEXCEPT(true)
		{
			return this->_name;
		}
		void DataString::setName(const std::string name) NOEXCEPT(true)
		{
			this->_name = name;
		}
		const void* DataString::getData(void) const NOEXCEPT(true)
		{
			return (void*)&this->_data;
		}
		const std::string DataString::getDataAsString(void) const NOEXCEPT(true)
		{
			return this->_data;
		}
		void DataString::setData(const void* data) NOEXCEPT(true)
		{
			this->_data = *(std::string*)data;
		}
		void DataString::setDataAsString(const std::string data) NOEXCEPT(true)
		{
			this->_data = data;
		}
		DataTypes DataString::getType(void) const NOEXCEPT(true)
		{
			return DataTypes::STRING;
		}
		std::string DataString::toString(void) const NOEXCEPT(false)
		{
			std::string s = "";
			for (const char* c = &(this->_data[0]); c[0] != 0x00; c++)
			{
				switch (c[0])
				{
					case '\'': s.append("\\'"); break;
					case '"': s.append("\\\""); break;
					case '\r': s.append("\\r"); break;
					case '\n': s.append("\\n"); break;
					case '\t': s.append("\\t"); break;
					case '\\': s.append("\\\\"); break;
					case ']': s.append("\\]"); break;
					case '[': s.append("\\["); break;
					default: s.append(c, c + 1); break;
				}
			}
			return std::string("\"").append(s).append("\"");
		}
	};
};