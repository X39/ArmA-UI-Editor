#pragma once
#include "Base.h"
namespace sqf
{
	class Scalar : public Base
	{
	private:
		long double _value;
	public:
		Scalar::Scalar(long double Value = 0) : _value(Value) {}
		Scalar::~Scalar() {}

		inline Type Scalar::getType(void) const
		{
			return Type::SCALAR;
		}
		inline std::string Scalar::escapedString(void) const
		{
			return std::to_string(this->getValue());
		}
		inline long double Scalar::getValue(void) const
		{
			return this->_value;
		}
		inline void Scalar::setValue(long double val)
		{
			this->_value = val;
		}
		static unsigned int Scalar::parsePartially(Scalar* out, std::string in)
		{
			size_t numberLen = 0;
			for (; numberLen < in.length(); numberLen++)
			{
				char c = in[numberLen];
				if (c != '.' && !(c >= '0' && c <= '9'))
					break;
			}
			if (numberLen >= in.length())
				throw std::exception("Unexpected \\0 before Parsing was complete");
			if (numberLen == 0)
				throw std::exception("Provided String contains no number at index 0");
			out->setValue(std::atof(in.substr(0, numberLen).c_str()));
			return numberLen - 1;
		}
	};
}