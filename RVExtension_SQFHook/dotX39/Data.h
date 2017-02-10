#pragma once
#include "visualStudioDefines.h"
#include <string>
namespace dotX39
{
	enum DataTypes
	{
		NA = -1,
		STRING = 0,
		SCALAR,
		DATETIME,
		BOOLEAN,
		ARRAY
	};
	class Data
	{
	public:
		virtual ~Data(void) {};
		virtual std::string getName(void) const = 0;
		virtual void setName(const std::string name) = 0;
		virtual const void* getData(void) const = 0;
		virtual void setData(const void* data) = 0;
		virtual DataTypes getType(void) const = 0;
		virtual std::string toString(void) const = 0;
	};
};