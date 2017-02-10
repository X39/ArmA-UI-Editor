#pragma once
#include "Data.h"
namespace dotX39
{
	class DataScalar : public Data
	{
	private:
		long double _data;
		std::string _name;
	public:
		DataScalar::DataScalar(long double data = 0, std::string name = "NOTSET")
		{
			_data = data;
			_name = name;
		}
		DataScalar::~DataScalar(void)
		{
			
		}
		std::string DataScalar::getName(void) const
		{
			return this->_name;
		}
		void DataScalar::setName(const std::string name)
		{
			this->_name = name;
		}
		const void* DataScalar::getData(void) const
		{
			return (void*)&this->_data;
		}
		const long double DataScalar::getDataAsLongDouble(void) const
		{
			return this->_data;
		}
		void DataScalar::setData(const void* data)
		{
			this->_data = *(long double*)data;
		}
		void DataScalar::setDataAsLongDouble(long double data)
		{
			this->_data = data;
		}
		DataTypes DataScalar::getType(void) const
		{
			return DataTypes::SCALAR;
		}
		std::string DataScalar::toString(void) const
		{
			if (this->_data - (long long) this->_data == 0)
				return std::to_string((long long) this->_data);
			return std::to_string(this->_data);
		}
	};
};