#pragma once
#include "Base.h"
#include "Scalar.h"
#include "Boolean.h"
#include "String.h"
#include "Nil.h"
#include <vector>
namespace sqf
{
	class Array : public Base
	{
	private:
		bool _preventAutoDeleteOnArrayDecostruction;
		std::vector<Base*> _arrayList;
	public:

		Array::Array(bool PreventAutoDeleteOnArrayDecostruction = false) : _preventAutoDeleteOnArrayDecostruction(PreventAutoDeleteOnArrayDecostruction) {}
		Array::~Array() 
		{
			if (!_preventAutoDeleteOnArrayDecostruction)
			{
				for (auto& it : this->_arrayList)
				{
					delete it;
				}
				this->_arrayList.clear();
			}
		}
		inline Type Array::getType(void) const
		{
			return Type::ARRAY;
		}
		std::string Array::escapedString(void) const
		{
			std::string s = "[";
			for (int i = 0; i < this->_arrayList.size(); i++)
			{
				if (i != 0)
					s.append(",");
				s.append(this->_arrayList[i]->escapedString());
			}
			return s.append("]");
		}
		inline size_t Array::length(void) const
		{
			return this->_arrayList.size();
		}
		inline Base* Array::push_back(Base* obj)
		{
			this->_arrayList.push_back(obj);
			return obj;
		}
		inline void Array::pop_back(void)
		{
			this->_arrayList.pop_back();
		}
		Base* Array::operator[](size_t i)
		{
			size_t len = this->length();
			if (i >= len)
				throw std::out_of_range("Given index is out of range");
			return this->_arrayList[i];
		}
		static unsigned int Array::parsePartially(Array* out, std::string in)
		{
			if (in[0] != '[')
				throw std::exception("Provided string is no Array");
			for (int i = 1; i < in.length(); i++)
			{
				char c = in[i];
				switch (c)
				{
				case ']': //End of this array
					return i;
					break;
				case '[': //New array here
					{
						Array* obj = new Array();
						try
						{
							i += Array::parsePartially(obj, in.substr(i));
						}
						catch (std::exception e)
						{
							delete obj;
							throw e;
						}
						out->_arrayList.push_back(obj);
					}
					break;
				case 't': case 'f': case 'T': case 'F': //Most likely a boolean here
				{
					Boolean* obj = new Boolean();
					try
					{
						i += Boolean::parsePartially(obj, in.substr(i));
					}
					catch (std::exception e)
					{
						delete obj;
						throw e;
					}
					out->_arrayList.push_back(obj);
				}
				break;
				case 'n': case 'N': //Most likely a nil value here
				{
					out->_arrayList.push_back(new Nil());
					i += 2;
				}
				break;
				case '"': //We discovered a string
					{
						String* obj = new String();
						try
						{
							i += String::parsePartially(obj, in.substr(i));
						}
						catch (std::exception e)
						{
							delete obj;
							throw e;
						}
						out->_arrayList.push_back(obj);
					}
					break;
				case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9': case '.': //Scalar value
					{
						Scalar* obj = new Scalar();
						try
						{
							i += Scalar::parsePartially(obj, in.substr(i));
						}
						catch (std::exception e)
						{
							delete obj;
							throw e;
						}
						out->_arrayList.push_back(obj);
					}
					break;

				case ',': //Next element
					break;
				case ' ': case '\t': //Whitespaces
					break;
				default:
					throw std::exception(std::string("Encountered unknown DataType, rest of the line: ").append(in.substr(i)).c_str());
				}
			}
			throw std::exception("Unexpected \\0 before Parsing was complete");
		}
	};
}