#pragma once
#include "visualStudioDefines.h"
#include "Data.h"
#include <string>
#include <vector>
namespace dotX39
{
	class Node
	{
	private:
		std::string _name;
		std::vector<Data*> _dataList;
		std::vector<Data*> _argList;
		std::vector<Node*> _nodeList;
	public:
		Node(std::string s);
		~Node();

		void addSubnode(Node* obj);
		void addArgument(Data* obj);
		void addData(Data* obj);
		const Data* getData(unsigned int index) const;
		const std::vector<Data*>* getDataArray(void) const;
		unsigned int getDataCount(void) const;
		const Data* getArgument(unsigned int index) const;
		const std::vector<Data*>* getArgumentArray(void) const;
		unsigned int getArgumentCount(void) const;
		const Node* getNode(unsigned int index) const;
		const std::vector<Node*>* getNodeArray(void) const;
		unsigned int getNodeCount(void) const;
		std::string getName(void) const;
	};
};