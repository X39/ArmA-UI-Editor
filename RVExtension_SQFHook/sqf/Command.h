#pragma once
#include "Array.h"
#include <vector>
#include <functional>
namespace sqf
{
	class Command
	{
	private:
		std::function<std::string(sqf::Array*, char*, int)> _fnc;
		sqf::Array* _parameters;
		std::string _help;
		std::string _name;
		bool _preventCompare;
	public:
		enum error
		{
			OK = 0,
			LengthDiffers = 1,
			TypeDiffers
		};
		Command(std::string name, std::function<std::string(sqf::Array*, char*, int)> fnc, std::string example = "", std::string help = "", bool preventCompare = false) : _fnc(fnc), _help(help), _name(name), _preventCompare(preventCompare)
		{
			_parameters = new sqf::Array;
			try
			{
				sqf::Array::parsePartially(_parameters, example);
			}
			catch (std::exception e)
			{
				delete _parameters;
				throw e;
			}
		}
		Command(Command& that) = delete;
		~Command(void)
		{
			delete _parameters;
		}
		error compareParameters(sqf::Array* paramA, sqf::Array* paramB = nullptr)
		{
			if (!paramB)
				paramB = _parameters;
			if (paramA->length() != paramB->length())
				return error::LengthDiffers;
			for (int i = 0; i < paramA->length(); i++)
			{
				if ((*paramA)[i]->getType() != (*paramB)[i]->getType())
					return error::TypeDiffers;
				if ((*paramA)[i]->getType() == sqf::Type::ARRAY)
				{
					auto result = compareParameters((sqf::Array*)(*paramB)[i], (sqf::Array*)(*paramB)[i]);
					if (result != error::OK)
						return result;
				}
			}
			return error::OK;
		}
		std::string getHelp(void)
		{
			return std::string(this->_name).append(": ").append(this->_help).append(" Example: ").append(this->_parameters->escapedString());
		}
		std::string runCommand(sqf::Array* params, char* output, int outputSize, bool checkParams = true)
		{
			if (checkParams && !this->_preventCompare && compareParameters(params))
				throw std::exception("Parameters dont match");
			auto result = this->_fnc(params, output, outputSize);
			return result;
		}
		inline bool runIfMatch(const std::string& name, std::string& out, sqf::Array* params, char* output, int outputSize, bool checkParams = true)
		{
			if (name.compare(this->_name) == 0)
			{
				out = runCommand(params, output, outputSize, checkParams);
				return true;
			}
			return false;
		}
	};
}