#pragma once
#include "visualStudioDefines.h"
#include "Node.h"
#include "Data.h"
#include <string>
#include <fstream>
namespace dotX39
{
	namespace DocumentReader
	{
		void readDocument(std::fstream&, Node*);
		inline void readDocument(const std::string filePath, Node* out) NOEXCEPT(false)
		{
			std::fstream stream(filePath, std::fstream::in);
			readDocument(stream, out);
			stream.close();
		}
		Data* readString(const std::string, const std::string);
		Data* readScalar(const std::string, const std::string);
		Data* readDateTime(const std::string, const std::string);
		Data* readBoolean(const std::string, const std::string);
		Data* readArray(const std::string, const std::string);
	};
};