#pragma once
#include "visualStudioDefines.h"
#include "Node.h"
#include "Data.h"
#include <string>
#include <fstream>
namespace dotX39
{
	namespace DocumentWriter
	{
		void writeDocument(std::ofstream&, const Node*, unsigned int tabCount = 0);
		inline void writeDocument(const std::string filePath, const Node* documentNode) NOEXCEPT(false)
		{
			std::ofstream stream(filePath, std::ofstream::binary);
			writeDocument(stream, documentNode);
			stream.close();
		}
		void writeString(std::ofstream&, const dotX39::Data&);
		void writeScalar(std::ofstream&, const dotX39::Data&);
		void writeDateTime(std::ofstream&, const dotX39::Data&);
		void writeBoolean(std::ofstream&, const dotX39::Data&);
		void writeArray(std::ofstream&, const dotX39::Data&);
	};
};