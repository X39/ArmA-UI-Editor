#include "DocumentWriter.h"
#include "DataArray.h"
#include "DataBoolean.h"
#include "DataDateTime.h"
#include "DataScalar.h"
#include "DataString.h"
#include <fstream>
#include <vector>

using namespace std;
//#define WRITE(STREAM, STR) STREAM ## .write( STR , strlen( STR ) ); STREAM ## .flush()
namespace dotX39
{
	namespace DocumentWriter
	{
		inline void WRITE(std::ofstream& stream, const char* str)
		{
			for (unsigned int i = 0; i < strlen(str); i++)
				stream.put(str[i]);
		}
		void writeDocument(std::ofstream& stream, const Node* doc, unsigned int tabCount) NOEXCEPT(false)
		{
			unsigned int imax;
			unsigned int i;
			//Print node name
			if (tabCount != 0 || (doc->getArgumentCount() > 0 || doc->getDataCount() > 0))
				WRITE(stream, string(tabCount, '\t').append(doc->getName()).c_str());
			if (doc->getArgumentCount() > 0)
			{
				//Iterate through all Arguemnts of current node
				WRITE(stream, "(");
				imax = doc->getArgumentCount();
				for (i = 0; i < imax; i++)
				{
					//Print next Argument of current node
					const Data* d = doc->getArgument(i);
					if (i > 0)
						WRITE(stream, ", ");
					WRITE(stream, d->getName().c_str());
					WRITE(stream, " = ");
					switch (d->getType())
					{
					case DataTypes::ARRAY: writeArray(stream, *d); break;
					case DataTypes::BOOLEAN: writeBoolean(stream, *d); break;
					case DataTypes::DATETIME: writeDateTime(stream, *d); break;
					case DataTypes::SCALAR: writeScalar(stream, *d); break;
					case DataTypes::STRING: writeString(stream, *d); break;
					default: stream.flush(); throw exception("One or more elements in a node are of unknown type");
					}
				}
				//Close argument block
				WRITE(stream, ")");
			}
			if (doc->getDataCount() > 0 || doc->getNodeCount() > 0)
			{
				//Open current node
				if (tabCount != 0 || (doc->getArgumentCount() > 0 || doc->getDataCount() > 0))
				{
					WRITE(stream, "\r\n");
					WRITE(stream, string(tabCount, '\t').append("{\r\n").c_str());
				}

				//Iterate through all Data containers of this node
				imax = doc->getDataCount();
				for (i = 0; i < imax; i++)
				{
					//Write next Data container
					const Data* d = doc->getData(i);
					WRITE(stream, string(tabCount + 1, '\t').append(d->getName()).c_str());
					WRITE(stream, " = ");
					switch (d->getType())
					{
						case DataTypes::ARRAY: writeArray(stream, *d); break;
						case DataTypes::BOOLEAN: writeBoolean(stream, *d); break;
						case DataTypes::DATETIME: writeDateTime(stream, *d); break;
						case DataTypes::SCALAR: writeScalar(stream, *d); break;
						case DataTypes::STRING: writeString(stream, *d); break;
						default: stream.flush(); throw exception("One or more elements in a node are of unknown type");
					}
					WRITE(stream, ";\r\n");
				}
				//Iterate through all child nodes of this node
				imax = doc->getNodeCount();
				for (i = 0; i < imax; i++)
				{
					//Write next child node
					writeDocument(stream, doc->getNode(i), tabCount + 1);
				}
				//Finally close current node
				if (tabCount != 0 || (doc->getArgumentCount() > 0 || doc->getDataCount() > 0))
					WRITE(stream, string(tabCount, '\t').append("}\r\n").c_str());
			}
			else
			{
				//We did not needed to write anything so terminate this node without a body
				if (tabCount != 0 || (doc->getArgumentCount() > 0 || doc->getDataCount() > 0))
					WRITE(stream, ";\r\n");
			}
			if (tabCount == 0)
				stream.flush();
		}
		void writeString(std::ofstream& stream, const dotX39::Data& data)
		{
			if (data.getType() != DataTypes::STRING)
				throw exception("Cannot write non-string Data container in writeString function");
			WRITE(stream, data.toString().c_str());
		}
		void writeScalar(std::ofstream& stream, const dotX39::Data& data)
		{
			if (data.getType() != DataTypes::SCALAR)
				throw exception("Cannot write non-scalar Data container in writeScalar function");
			WRITE(stream, data.toString().c_str());
		}
		void writeDateTime(std::ofstream& stream, const dotX39::Data& data)
		{
			if (data.getType() != DataTypes::DATETIME)
				throw exception("Cannot write non-datetime Data container in writeDateTime function");
			WRITE(stream, data.toString().c_str());
		}
		void writeBoolean(std::ofstream& stream, const dotX39::Data& data)
		{
			if (data.getType() != DataTypes::BOOLEAN)
				throw exception("Cannot write non-boolean Data container in writeBoolean function");
			WRITE(stream, data.toString().c_str());
		}
		void writeArray(std::ofstream& stream, const dotX39::Data& data)
		{
			if (data.getType() != DataTypes::ARRAY)
				throw exception("Cannot write non-array Data container in writeArray function");
			WRITE(stream, data.toString().c_str());
		}
	};
};