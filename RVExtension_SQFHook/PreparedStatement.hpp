#pragma once
#include <string>
#include <vector>
#include "sqf\Array.h"
class PreparedStatement
{
public:
	typedef struct structArgument
	{
		std::string name;
		std::string token;
		bool isEscaped;
		structArgument(std::string Name, std::string Token, bool IsEscaped = false) : name(Name), token(Token), isEscaped(IsEscaped) {};
	} ARGUMENT;
private:

	std::string _name;
	std::string _statement;
	std::vector<ARGUMENT> _arguments;

	ARGUMENT* getArgumentByName(const char* name);

public:
	PreparedStatement(std::string Name, std::string Statement);

	~PreparedStatement();

	std::string getName(void);

	/*
	 Adds an Argument to the list of arguments of this PreparedStatement
	 
	 @Param arg				The argument to add
	*/
	void addArgument(ARGUMENT& arg);

	/*
	 Constructs the statement string represented by this PreparedStatement
	 
	 @Param argumentArray	Array containing informations about the arguments to be replaced
							Structure [<argumentName1>, <argument1>, <argumentName2>, <argument2>, <argumentNameN>, <argumentN>]
	 @Param argumentCount	Contains the ammount of elements represented by argumentArray
	 @Param outString		The string that will be given out
	 @Return				The constructed statement

	 @Throws				std::exception if it hits something that is not as expected
	*/
	std::string getStatementString(const char** argumentArray, unsigned int argumentCount);
	std::string getStatementString(sqf::Array& arguments);
};

