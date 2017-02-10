#include "PreparedStatement.hpp"
#include <exception>


PreparedStatement::PreparedStatement(std::string Name, std::string Statement)
{
	_name = Name;
	_statement = Statement;
}

PreparedStatement::~PreparedStatement()
{
}

inline std::string PreparedStatement::getName(void)
{
	return this->_name;
}
void PreparedStatement::addArgument(ARGUMENT& arg)
{
	this->_arguments.push_back(arg);
}
PreparedStatement::ARGUMENT* PreparedStatement::getArgumentByName(const char* name)
{
	for (int i = 0; i < this->_arguments.size(); i++)
	{
		if (_arguments[i].name.compare(name) == 0)
			return &(this->_arguments[i]);
	}
	return nullptr;
}

std::string PreparedStatement::getStatementString(const char** argumentArray, unsigned int argumentCount)
{
	if (argumentCount % 2 != 0)
		throw std::exception(std::string(std::to_string(argumentCount)).append(" is no valid argument count (count % 2 == 0)").c_str());
	if (argumentCount / 2 != this->_arguments.size())
		throw std::exception(std::string("PreparedStatements '").append(this->_name).append("' ArgumentCount is ").append(std::to_string(this->_arguments.size())).append(" but got ").append(std::to_string(argumentCount / 2)).append(" arguments for replacement").c_str());

	//Simple Structure that contains informations about the Argument to replace and the text to replace the arguments token with
	struct ARG
	{
		ARGUMENT& argumentReference;
		const char* replacement;
	};
	std::vector<struct ARG> args;
	std::string outString;

	for (int i = 0; i < argumentCount; i += 2)
	{
		ARGUMENT* tmpArgument = getArgumentByName(argumentArray[i]);
		if (tmpArgument == nullptr)
			throw std::exception(std::string("Cannot find argument '").append(argumentArray[i]).append("' for PreparedStatement '").append(this->_name).append("'").c_str());
		struct ARG arg = { *tmpArgument, argumentArray[i + 1] };
		args.push_back(arg);
	}
	int lastFindResult = 0;
	for (auto& arg : args)
	{
		int findResult = this->_statement.find(arg.argumentReference.token, lastFindResult);
		if (findResult == -1)
			throw std::exception(std::string("Cannot find argument token '").append(arg.argumentReference.token).append("' inside of PreparedStatement '").append(this->_name).append("'").c_str());
		outString.append(this->_statement.substr(lastFindResult, findResult));
		outString.append(arg.replacement);
		lastFindResult = findResult;
	}
	outString.append(this->_statement.substr(lastFindResult));
	return outString;
}
std::string PreparedStatement::getStatementString(sqf::Array& arguments)
{
	auto argumentCount = arguments.length();
	if (argumentCount % 2 != 0)
		throw std::exception(std::string(std::to_string(argumentCount)).append(" is no valid argument count (count % 2 == 0)").c_str());
	if (argumentCount / 2 != this->_arguments.size())
		throw std::exception(std::string("PreparedStatements '").append(this->_name).append("' ArgumentCount is ").append(std::to_string(this->_arguments.size())).append(" but got ").append(std::to_string(argumentCount / 2)).append(" arguments for replacement").c_str());

	//Simple Structure that contains informations about the Argument to replace and the text to replace the arguments token with
	struct ARG
	{
		ARGUMENT& argumentReference;
		const char* replacement;
	};
	std::vector<struct ARG> args;
	std::string outString;

	for (int i = 0; i < argumentCount; i += 2)
	{
		ARGUMENT* tmpArgument = getArgumentByName(((sqf::String*)arguments[i])->getValue().c_str());
		if (tmpArgument == nullptr)
			throw std::exception(std::string("Cannot find argument '").append(((sqf::String*)arguments[i])->getValue().c_str()).append("' for PreparedStatement '").append(this->_name).append("'").c_str());
		struct ARG arg = { *tmpArgument, ((sqf::String*)arguments[i + 1])->getValue().c_str() };
		args.push_back(arg);
	}
	int lastFindResult = 0;
	for (auto& arg : args)
	{
		int findResult = this->_statement.find(arg.argumentReference.token, lastFindResult);
		if (findResult == -1)
			throw std::exception(std::string("Cannot find argument token '").append(arg.argumentReference.token).append("' inside of PreparedStatement '").append(this->_name).append("'").c_str());
		outString.append(this->_statement.substr(lastFindResult, findResult));
		outString.append(arg.replacement);
		lastFindResult = findResult;
	}
	outString.append(this->_statement.substr(lastFindResult));
	return outString;
}
