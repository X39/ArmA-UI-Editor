#include "Node.h"

namespace dotX39
{
	Node::Node(std::string s)
	{
		this->_name = (s.empty() ? "NOTSET" : s);
	}
	Node::~Node()
	{
		while (!this->_argList.empty()){ delete this->_argList.back(); this->_argList.pop_back(); }
		while (!this->_dataList.empty()){ delete this->_dataList.back(); this->_dataList.pop_back(); }
		while (!this->_nodeList.empty()){ delete this->_nodeList.back(); this->_nodeList.pop_back(); }
	}

	void Node::addSubnode(Node* obj)
	{
		this->_nodeList.push_back(obj);
	}
	void Node::addArgument(Data* obj)
	{
		this->_argList.push_back(obj);
	}
	void Node::addData(Data* obj)
	{
		this->_dataList.push_back(obj);
	}
	const std::vector<Data*>* Node::getDataArray(void) const
	{
		return &this->_dataList;
	}
	unsigned int Node::getDataCount(void) const
	{
		return this->_dataList.size();
	}
	const Data* Node::getData(unsigned int index) const
	{
		if (this->_dataList.size() <= index)
			return NULL;
		return this->_dataList[index];
	}
	const std::vector<Data*>* Node::getArgumentArray(void) const
	{
		return &this->_argList;
	}
	unsigned int Node::getArgumentCount(void) const
	{
		return this->_argList.size();
	}
	const Data* Node::getArgument(unsigned int index) const
	{
		if (this->_argList.size() <= index)
			return NULL;
		return this->_argList[index];
	}
	const std::vector<Node*>* Node::getNodeArray(void) const
	{
		return &this->_nodeList;
	}
	unsigned int Node::getNodeCount(void) const
	{
		return this->_nodeList.size();
	}
	const Node* Node::getNode(unsigned int index) const
	{
		if (this->_nodeList.size() <= index)
			return NULL;
		return this->_nodeList[index];
	}
	std::string Node::getName(void) const
	{
		return this->_name;
	}
};