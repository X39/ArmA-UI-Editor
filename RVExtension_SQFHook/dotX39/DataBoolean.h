#pragma once
#include "Data.h"
namespace dotX39
{
	class DataBoolean : public Data
	{
	private:
		bool _data;
		std::string _name;
	public:
		DataBoolean::DataBoolean(bool data = false, std::string name = "NOTSET")
		{
			_data = data;
			_name = name;
		}
		DataBoolean::~DataBoolean(void)
		{
			
		}
		std::string DataBoolean::getName(void) const
		{
			return this->_name;
		}
		void DataBoolean::setName(const std::string name)
		{
			this->_name = name;
		}
		const void* DataBoolean::getData(void) const
		{
			return (void*)&this->_data;
		}
		const bool DataBoolean::getDataAsBoolean(void) const
		{
			return this->_data;
		}
		void DataBoolean::setData(const void* data)
		{
			this->_data = *(bool*)data;
		}
		void DataBoolean::setDataAsBoolean(bool data)
		{
			this->_data = data;
		}
		DataTypes DataBoolean::getType(void) const
		{
			return DataTypes::BOOLEAN;
		}
		std::string DataBoolean::toString(void) const
		{
			return (this->_data ? "TRUE" : "FALSE");
		}
	};
};