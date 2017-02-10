#pragma once
#include "Data.h"
namespace dotX39
{
	class DataDateTime : public Data
	{
	private:
		unsigned short _day;
		unsigned short _month;
		unsigned short _year;
		unsigned short _hour;
		unsigned short _minute;
		unsigned short _second;
		std::string _name;
	public:
		DataDateTime::DataDateTime(unsigned short day = 0, unsigned short month = 0, unsigned short year = 0, unsigned short hour = 0, unsigned short minute = 0, unsigned short second = 0, std::string name = "NOTSET")
		{
			this->_day = day;
			this->_month = month;
			this->_year = year;
			this->_hour = hour;
			this->_minute = minute;
			this->_second = second;
			this->_name = name;
		}
		DataDateTime::~DataDateTime(void) { }
		std::string DataDateTime::getName(void) const
		{
			return this->_name;
		}
		void DataDateTime::setName(const std::string name)
		{
			this->_name = name;
		}
		const void* DataDateTime::getData(void) const { return NULL; }
		void DataDateTime::setData(const void* data) { }
		unsigned short DataDateTime::getDay(void) const		{ return this->_day; }
		unsigned short DataDateTime::getMonth(void) const	{ return this->_month; }
		unsigned short DataDateTime::getYear(void) const	{ return this->_year; }
		unsigned short DataDateTime::getHour(void) const	{ return this->_hour; }
		unsigned short DataDateTime::getMinute(void) const	{ return this->_minute; }
		unsigned short DataDateTime::getSecond(void) const	{ return this->_second; }
		void DataDateTime::setDay(unsigned short i)		{ this->_day = i; }
		void DataDateTime::setMonth(unsigned short i)		{ this->_month = i; }
		void DataDateTime::setYear(unsigned short i)		{ this->_year = i; }
		void DataDateTime::setHour(unsigned short i)		{ this->_hour = i; }
		void DataDateTime::setMinute(unsigned short i)	{ this->_minute = i; }
		void DataDateTime::setSecond(unsigned short i)	{ this->_second = i; }
		DataTypes DataDateTime::getType(void) const
		{
			return DataTypes::DATETIME;
		}
		std::string DataDateTime::toString(void) const
		{
			std::string s = "\\";
			s.append(std::to_string(this->_day)).append(".");
			s.append(std::to_string(this->_month)).append(".");
			s.append(std::to_string(this->_year)).append(" ");
			s.append(std::to_string(this->_hour)).append(":");
			s.append(std::to_string(this->_minute)).append(":");
			s.append(std::to_string(this->_second)).append("\\");
			return s;
		}
	};
};