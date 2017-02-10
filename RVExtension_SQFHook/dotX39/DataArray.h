#pragma once
#include "Data.h"
#include <vector>
namespace dotX39
{
	class DataArray : public Data
	{
	private:
		std::vector<Data*> _data;
		std::string _name;
	public:
		DataArray::DataArray(std::string s = "NOTSET")
		{
			_data = std::vector<Data*>();
			_name = s;
		}
		DataArray::~DataArray(void)
		{
			while (!this->_data.empty()){ delete (this->_data.back()); this->_data.pop_back(); }
		}
		std::string DataArray::getName(void) const
		{
			return this->_name;
		}
		void DataArray::setName(const std::string name)
		{
			this->_name = name;
		}
		const void* DataArray::getData(void) const { return NULL; }
		void DataArray::setData(const void* data) { }
		DataTypes DataArray::getType(void) const
		{
			return DataTypes::ARRAY;
		}
		void DataArray::addDataElement(Data* elm)
		{
			this->_data.push_back(elm);
		}
		Data* DataArray::getDataElement(unsigned int index) const
		{
			if (this->_data.size() <= index)
				return NULL;
			return this->_data[index];
		}
		unsigned int DataArray::getDataCount(void) const
		{
			return this->_data.size();
		}
		std::string DataArray::toString(void) const
		{
			std::string s = "[";
			for (unsigned int i = 0; i < this->_data.size(); i++)
			{
				if (i != 0)
					s.append(", ");
				s.append(this->_data[i]->toString());
			}
			s.append("]");
			return s;
		}
	};
};