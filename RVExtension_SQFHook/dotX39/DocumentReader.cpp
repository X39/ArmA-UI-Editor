#include "DocumentReader.h"
#include "DataArray.h"
#include "DataBoolean.h"
#include "DataDateTime.h"
#include "DataScalar.h"
#include "DataString.h"
#include <fstream>
#include <vector>
#include <algorithm> 
#include <functional> 
#include <cctype>
#include <locale>

using namespace std;
#define READINGMODE_NAME 0
#define READINGMODE_DATA 1
#define READINGMODE_ARGUMENT 2
#define READINGMODE_ARGUMENT_END 3
#define READER_BUFFERSIZE 256
#define SETREADINGMODE(MODE) setReadingMode(MODE, &readingMode, &readingMode_last)
#define DISABLEOUTERCATCH
namespace dotX39
{
	namespace DocumentReader
	{
		//Following 3 functions are 1:1 from http://stackoverflow.com/a/217605/2684203
		// trim from start
		static inline std::string &ltrim(std::string &s) {
			s.erase(s.begin(), std::find_if(s.begin(), s.end(), std::not1(std::ptr_fun<int, int>(std::isspace))));
			return s;
		}
		// trim from end
		static inline std::string &rtrim(std::string &s) {
			s.erase(std::find_if(s.rbegin(), s.rend(), std::not1(std::ptr_fun<int, int>(std::isspace))).base(), s.end());
			return s;
		}
		// trim from both ends
		static inline std::string &trim(std::string &s) {
			return ltrim(rtrim(s));
		}

		static inline void setReadingMode(unsigned int mode, unsigned int* out1, unsigned int* out2)
		{
			*out2 = *out1;
			*out1 = mode;
		}
		void readDocument(std::fstream& doc, Node* out) NOEXCEPT(false)
		{//TODO: adjust the "throw" so that ALL pointer vectors are deleted!
			char s[READER_BUFFERSIZE];
			s[READER_BUFFERSIZE - 1] = '\0';
			char* c = 0x00;
			int arrayCounter = 0;
			vector<Node*> curNodeTree;
			curNodeTree.push_back(out);
			bool commentFlag = false;
			unsigned int readingMode = READINGMODE_NAME;
			unsigned int readingMode_last = READINGMODE_NAME;
			unsigned long lineNumber = 1;
			string name;
			string argumentName;
			bool dataTag = false;
			DataTypes dataType = NA;
			string data;
			vector<Data*> argumentsData;
			if (!doc.is_open())
				throw exception("Cannot open file");
#ifndef DISABLEOUTERCATCH
			try
			{
#endif
				while (!doc.eof())
				{
					char stringDetectionChar;
					doc.read(s, READER_BUFFERSIZE - 1);
					auto gcount = doc.gcount();
					s[gcount] = '\0';
					for (int i = 0; i < READER_BUFFERSIZE; i++)
					{
						if (curNodeTree.size() < 1)
							throw exception("While parsing the document, the root node got closed");
						c = s + i;
						if (c[0] == '\0')
							break;
						if (c[0] == '\n')
							lineNumber++;
						if (commentFlag)
						{
							if (c[0] == '*' && c[1] == '/')
							{
								commentFlag = false;
								i++;
							}
							continue;
						}
						if (readingMode == READINGMODE_NAME)
						{
							if (c[0] == ';')
							{
								Node* n = new Node(name);
								for (unsigned int j = 0; j < argumentsData.size(); j++)
									n->addArgument(argumentsData[j]);
								argumentsData.clear();
								curNodeTree.back()->addSubnode(n);
								name.clear();
								continue;
							}
#pragma region node arguments
							if (c[0] == '(')
							{
								//We located an argument
								SETREADINGMODE(READINGMODE_ARGUMENT);
								continue;
							}
#pragma endregion
#pragma region new node
							if (c[0] == '{')
							{
								//We located a new node here
								Node* n = new Node(name);
								for (unsigned int j = 0; j < argumentsData.size(); j++)
									n->addArgument(argumentsData[j]);
								argumentsData.clear();
								curNodeTree.back()->addSubnode(n);
								curNodeTree.push_back(n);
								name.clear();
								continue;
							}
							else if (c[0] == '}')
							{
								//Current node wants to be closed
								curNodeTree.pop_back();
								continue;
							}
#pragma endregion
#pragma region new option data
							else if (c[0] == '=')
							{
								//We located some data for our node here
								SETREADINGMODE(READINGMODE_DATA);
								continue;
							}
#pragma endregion
#pragma region new character for name
							else if (c[0] == '/' && c[1] == '*')
							{
								commentFlag = true;
								continue;
							}
							else if (true)
							{
								//just another char ... lets add it
								if (isalnum(c[0]))
								{
									name.append(c, c + 1);
									continue;
								}
								else if (iscntrl(c[0]) || c[0] == ' ')
								{
									//just discard it as it is not allowed for node names
									continue;
								}
								else
								{
									throw exception("found non alphanumeric char for node name, please correct the file");
								}
							}
#pragma endregion
							continue;
						}
						else if (readingMode == READINGMODE_DATA)
						{
#pragma region define datatype
							if (!dataTag)
							{//Searching DataTag
								if (iscntrl(c[0]) || c[0] == ' '){ continue; }
								else if (c[0] == '['){ dataTag = true; dataType = DataTypes::ARRAY; i--; }
								else if (c[0] == '"'){ dataTag = true; dataType = DataTypes::STRING; i--; }
								else if (c[0] == '\''){ dataTag = true; dataType = DataTypes::STRING; i--; }
								else if (c[0] == '\\'){ dataTag = true; dataType = DataTypes::DATETIME; i--; }
								else if (c[0] >= '0' && c[0] <= '9'){ dataTag = true; dataType = DataTypes::SCALAR; i--; }
								else if (c[0] == 'f' || c[0] == 'F' || c[0] == 't' || c[0] == 'T'){ dataTag = true; dataType = DataTypes::BOOLEAN; i--; }

								else if (c[0] == '(' || c[0] == ')'){ throw exception("Invalid argument tag when data tag was expected"); }
								else if (c[0] == '{' || c[0] == '}'){ throw exception("Invalid node tag when data tag was expected"); }
								else if (c[0] == ']'){ throw exception("Invalid close array tag when open array (or another data) tag was expected"); }
								else if (c[0] == ';'){ throw exception("Invalid end of line tag when data tag was expected"); }
								else if (c[0] == ':'){ throw exception("Invalid time seperator tag when data tag was expected"); }
								else if (c[0] == '.'){ throw exception("Invalid scalar/date seperator tag when data tag was expected"); }
								else if (c[0] == ','){ throw exception("Invalid seperator tag when data tag was expected"); }
								else { throw exception("Invalid unknown tag when data tag was expected"); }
								continue;
							}
#pragma endregion

#pragma region parse data
							else
							{
								if (dataType == DataTypes::ARRAY)
								{
									char* cp = c;
									char* cp2;

									while (true)
									{
										//ToDo: Improove array detection to allow strings to have non-escaped ']' and '['
										if (cp[0] == '[' || cp[0] == ']')
										{
											cp2 = cp;
											int j = 0;
											for (cp2 = cp; cp2[-1] == '\\'; cp2--)
												j++;
											if (j % 2 != 1)
											{
												if (cp[0] == '[')
													arrayCounter++;
												else
													arrayCounter--;
												if (arrayCounter == 0)
													break;
											}
										}
										cp++;
										if (cp[0] == NULL)
											break;
									}
									if (cp[0] != NULL)
									{
										data.append(c, cp + 1);
										i += strlen(c) - strlen(cp) + 1;
										c = cp + 1;
										while ((iscntrl(c[0]) || c[0] == ' ' || c[0] == '\t') && c[0] != '\0')
										{
											c = c + 1;
											i++;
										}
										if (((c[0] != ';' && readingMode_last == READINGMODE_NAME) || (c[0] != ',' && c[0] != ')' && readingMode_last == READINGMODE_ARGUMENT)) && c[0] != '\0')
											throw exception(string("Unexpected character '").append(c, c + 1).append("' where lineTerminator should have been").c_str());
										if (readingMode_last == READINGMODE_NAME)
										{
											curNodeTree.back()->addData(readArray(data, name));
											name.clear();
										}
										else
										{
											argumentsData.push_back(readArray(data, argumentName));
											argumentName.clear();
										}
										SETREADINGMODE(readingMode_last);
										dataType = DataTypes::NA;
										dataTag = false;
										data.clear();
										arrayCounter = 0;
									}
									else
									{
										data.append(c);
										i = READER_BUFFERSIZE;
									}
								}
								else if (dataType == DataTypes::BOOLEAN)
								{
									char* cp;
									cp = strchr(c, 'e');
									if (cp != NULL)
									{
										cp++;
										data.append(c, cp);
										i += strlen(c) - strlen(cp) - 1;
										c = cp;
										while ((iscntrl(c[0]) || c[0] == ' ' || c[0] == '\t') && c[0] != '\0')
										{
											c = c + 1;
											i++;
										}
										if (((c[0] != ';' && readingMode_last == READINGMODE_NAME) || (c[0] != ',' && c[0] != ')' && readingMode_last == READINGMODE_ARGUMENT)) && c[0] != '\0')
											throw exception(string("Unexpected character '").append(c, c + 1).append("' where lineTerminator should have been").c_str());
										if (readingMode_last == READINGMODE_NAME)
										{
											curNodeTree.back()->addData(readBoolean(data, name));
											name.clear();
										}
										else
										{
											argumentsData.push_back(readBoolean(data, argumentName));
											argumentName.clear();
										}
										SETREADINGMODE(readingMode_last);
										dataType = DataTypes::NA;
										dataTag = false;
										data.clear();
									}
									else
									{
										data.append(c);
										i = READER_BUFFERSIZE;
									}
								}
								else if (dataType == DataTypes::DATETIME)
								{
									char* cp;
									cp = strchr(c + 1, '\\');
									if (cp != NULL)
									{
										data.append(c, cp + 1);
										i += strlen(c) - strlen(cp) + 1;
										c = cp + 1;
										while ((iscntrl(c[0]) || c[0] == ' ' || c[0] == '\t') && c[0] != '\0')
										{
											c = c + 1;
											i++;
										}
										if (((c[0] != ';' && readingMode_last == READINGMODE_NAME) || (c[0] != ',' && c[0] != ')' && readingMode_last == READINGMODE_ARGUMENT)) && c[0] != '\0')
											throw exception(string("Unexpected character '").append(c, c + 1).append("' where lineTerminator should have been").c_str());
										if (readingMode_last == READINGMODE_NAME)
										{
											curNodeTree.back()->addData(readDateTime(data, name));
											name.clear();
										}
										else
										{
											argumentsData.push_back(readDateTime(data, argumentName));
											argumentName.clear();
										}
										SETREADINGMODE(readingMode_last);
										dataType = DataTypes::NA;
										dataTag = false;
										data.clear();
									}
									else
									{
										data.append(c);
										i = READER_BUFFERSIZE;
									}
								}
								else if (dataType == DataTypes::SCALAR)
								{
									char* cp;
									char* cp2 = c;
									while (cp2[0] != '\0')
									{
										if (cp2[0] == '.' || (cp2[0] >= '0' && cp2[0] <= '9'))
											cp = cp2;
										else
											break;
										cp2++;
									}
									if (cp != NULL)
									{
										data.append(c, cp + 1);
										i += strlen(c) - strlen(cp + 1);
										c = cp + 1;
										while ((iscntrl(c[0]) || c[0] == ' ' || c[0] == '\t') && c[0] != '\0')
										{
											c = c + 1;
											i++;
										}
										if (((c[0] != ';' && readingMode_last == READINGMODE_NAME) || (c[0] != ',' && c[0] != ')' && readingMode_last == READINGMODE_ARGUMENT)) && c[0] != '\0')
											throw exception(string("Unexpected character '").append(c, c + 1).append("' where lineTerminator should have been").c_str());
										if (readingMode_last == READINGMODE_NAME)
										{
											curNodeTree.back()->addData(readScalar(data, name));
											name.clear();
										}
										else
										{
											argumentsData.push_back(readScalar(data, argumentName));
											argumentName.clear();
										}
										SETREADINGMODE(readingMode_last);
										dataType = DataTypes::NA;
										dataTag = false;
										data.clear();
									}
									else
									{
										data.append(c);
										i = READER_BUFFERSIZE;
									}
								}
								else if (dataType == DataTypes::STRING)
								{
									char* cp;
									char* cp2;
									char* cp3 = c;
									int j = 0;
									if (data.empty())
									{
										stringDetectionChar = c[0];
										cp3++;
									}
									while (true)
									{
										cp = strchr(cp3, stringDetectionChar);
										if (cp == NULL)
											break;
										for (cp2 = cp; cp2[-1] == '\\'; cp2--)
											j++;
										if (j % 2 != 1)
											break;
										cp3 = cp + 1;
									}
									if (cp != NULL)
									{
										data.append(c, cp + 1);
										i += strlen(c) - strlen(cp);
										c = cp + 1;
										while ((iscntrl(c[0]) || c[0] == ' ' || c[0] == '\t') && c[0] != '\0')
										{
											c++;
											i++;
										}
										if (((c[0] != ';' && readingMode_last == READINGMODE_NAME) || (c[0] != ',' && c[0] != ')' && readingMode_last == READINGMODE_ARGUMENT)) && c[0] != '\0')
											throw exception(string("Unexpected character '").append(c, c+1).append("' where lineTerminator should have been").c_str());
										if (readingMode_last == READINGMODE_NAME)
										{
											curNodeTree.back()->addData(readString(data, name));
											name.clear();
											i++;
										}
										else
										{
											argumentsData.push_back(readString(data, argumentName));
											argumentName.clear();
										}
										SETREADINGMODE(readingMode_last);
										dataType = DataTypes::NA;
										dataTag = false;
										data.clear();
									}
									else
									{
										data.append(c);
										i = READER_BUFFERSIZE;
									}
								}
							}
							continue;
						}
#pragma endregion
#pragma region handle arguments
						else if (readingMode == READINGMODE_ARGUMENT)
						{
							//just another char ... lets add it
							if (c[0] == '=')
							{
								SETREADINGMODE(READINGMODE_DATA);
								continue;
							}
							if (c[0] == ')')
							{
								SETREADINGMODE(READINGMODE_ARGUMENT_END);
								continue;
							}
							if (isalnum(c[0]))
							{
								argumentName.append(c, c + 1);
								continue;
							}
							else if (iscntrl(c[0]) || c[0] == ' ')
							{
								//just discard it as it is not allowed for node names
								continue;
							}
							else if (c[0] == ',' && argumentName.empty())
							{
								continue;
							}
							else
							{
								throw exception("found non alphanumeric char for argument name, please correct the file");
							}
						}
						else if (readingMode == READINGMODE_ARGUMENT_END)
						{
							if (c[0] == ' ' || c[0] == '\r' || c[0] == '\n' || c[0] == '\t')
								continue;
							if (iscntrl(c[0]))
								continue;
							if (c[0] != ';' && c[0] != '{')
								throw exception("Undexpected character after arguments");
							if (c[0] == ';' || c[0] == '{')
								i--;
							SETREADINGMODE(READINGMODE_NAME);
						}
#pragma endregion
					}
				}
#ifndef DISABLEOUTERCATCH
			}
			catch (exception e)
			{
				throw new exception(string("Exception '").append(e.what()).append("' on line ").append(to_string(lineNumber)).c_str());
			}
#endif
			if (readingMode != READINGMODE_NAME)
				throw exception("Reached EOF before all parsing operations ended");
			if (curNodeTree.size() != 1)
				throw exception("Invalid node count at EOF");
		}
		Data* readString(const std::string data, const std::string name) NOEXCEPT(false)
		{
			std::string work = data;
			trim(work);
			if (work.length() < 2 || !((work[0] == '"' && work.back() == '"') || (work[0] == '\'' && work.back() == '\'')))
				throw exception("Invalid Arguments, Provided string contains no STRING dataType");
			work.erase(work.begin(), work.begin() + 1);
			work.erase(work.end() - 1, work.end());
			DataString* dataString = new DataString(work, name, true);
			return dataString;
		}
		Data* readScalar(const std::string data, const std::string name) NOEXCEPT(false)
		{
			return new DataScalar(stold(data), name);
		}
		Data* readDateTime(const std::string data, const std::string name) NOEXCEPT(false)
		{
			std::string work = data;
			trim(work);
			if (work.length() < 2 || work[0] != '\\' || work.back() != '\\')
				throw exception("Invalid Arguments, Provided string contains no DATETIME dataType");
			work.erase(work.begin(), work.begin() + 1);
			work.erase(work.end() - 1, work.end());
			DataDateTime* dateTime = new DataDateTime();
			dateTime->setName(name);
			try
			{
				if (work.find(".") != string::npos)
				{
					switch (std::count(work.begin(), work.end(), '.'))
					{
					case 2:
						dateTime->setDay(stoi(work));
						work.erase(work.begin(), work.begin() + work.find_first_of(".") + 1);
					case 1:
						dateTime->setMonth(stoi(work));
						work.erase(work.begin(), work.begin() + work.find_first_of(".") + 1);
					case 0:
						dateTime->setYear(stoi(work));
						if (work.find(' ') != string::npos)
						{
							work.erase(work.begin(), work.begin() + work.find_first_of(" ") + 1);
						}
						break;
					default:
						throw exception("Invalid Arguments, Provided string contains invalid DATETIME");
					}
				}
				if (work.find(':') != string::npos)
				{
					dateTime->setHour(stoi(work));
					if (work.find_first_of(":") != string::npos)
					{
						work.erase(work.begin(), work.begin() + work.find_first_of(":") + 1);
						dateTime->setMinute(stoi(work));
						if (work.find_first_of(":") != string::npos)
						{
							work.erase(work.begin(), work.begin() + work.find_first_of(":") + 1);
							dateTime->setSecond(stoi(work));
						}
					}
				}
			}
			catch (exception e)
			{
				delete dateTime;
				throw e;
			}
			return dateTime;
		}
		Data* readBoolean(const std::string data, const std::string name) NOEXCEPT(false)
		{
			std::string work = data;
			trim(work);
			bool flag = work[0] == 't' || work[0] == 'T';
			if (!flag && !(work[0] == 'f' || work[0] == 'F'))
				throw exception("Invalid Arguments, Provided string contains no BOOLEAN dataType");
			return new DataBoolean(flag, name);
		}
		Data* readArray(const std::string data, const std::string name) NOEXCEPT(false)
		{
			std::string work = data;
			trim(work);
			if (work.length() < 2 || work[0] != '[' || work.back() != ']')
				throw exception("Invalid Arguments, Provided string contains no STRING dataType");
			work.erase(work.begin(), work.begin() + 1);
			work.erase(work.end() - 1, work.end());
			DataArray* dataArray = new DataArray();
			dataArray->setName(name);
			try
			{
				while (!work.empty())
				{
					Data* d = NULL;
					size_t p = 0;
					switch (work[0])
					{
					case '\\':
						d = readDateTime(work.substr(0, work.find_first_of(",")), "");
						p = work.find_first_of(",");
						if (p != string::npos)
							work.erase(work.begin(), work.begin() + p);
						break;
					case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9':
						d = readScalar(work.substr(0, work.find_first_of(",")), "");
						p = work.find_first_of(",");
						if (p != string::npos)
							work.erase(work.begin(), work.begin() + p);
						else
							work.clear();
						break;
					case '"': case '\'':
						{
							auto cp = work.c_str() + 1;
							auto j = 0;
							auto index = 1;
							auto controlChar = work[0];
							while (cp[0] != '\0')
							{
								if ((cp[0] == controlChar) && j % 2 != 1)
									break;
								if (cp[0] == '\\')
									j++;
								else
									j = 0;
								cp++;
								index++;
							}
							d = readString(work.substr(0, index + 1), "");
							p = work.find_first_of(",");
							if (p != string::npos)
								work.erase(work.begin(), work.begin() + p);
							else
								work.clear();
						}
						break;
					case '[':
						{
							auto cp = work.c_str() + 1;
							auto j = 0;
							auto index = 1;
							auto arrayCounter = 1;
							while (cp[0] != '\0')
							{
								if (cp[0] == '[' || cp[0] == ']')
								{
									if (j % 2 != 1)
									{
										if (cp[0] == '[')
											arrayCounter++;
										else
											arrayCounter--;
										if (arrayCounter == 0)
											break;
									}
								}
								if (cp[0] == '\\')
									j++;
								else
									j = 0;
								cp++;
								index++;
							}
							d = readArray(work.substr(0, index + 1), "");
							work.erase(work.begin(), work.begin() + index + 1);
							p = work.find_first_of(",");
							if (p != string::npos)
								work.erase(work.begin(), work.begin() + p);
							else
								work.clear();
						}
						break;
					case 't': case 'T': case 'f': case 'F':
						d = readBoolean(work.substr(0, work.find_first_of(",")), "");
						p = work.find_first_of(",");
						if (p != string::npos)
							work.erase(work.begin(), work.begin() + p);
						else
							work.clear();
						break;
					case ',': case ' ': case '\0': case '\r': case '\n': case '\t':
						work.erase(work.begin(), work.begin() + 1);
						break;
					default:
						throw exception(string("Invalid Arguments, Provided string contains invalid syntax for array! Rest of the string: ").append(work).c_str());
					}
					if(d != NULL)
						dataArray->addDataElement(d);
				}
			}
			catch (exception e)
			{
				delete dataArray;
				throw e;
			}
			return dataArray;
		}
	};
};