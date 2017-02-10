#include "Connection.hpp"
#include <Windows.h>
#include <thread>


Connection::Connection(std::string name, std::string serverUri, std::string database, std::string username, std::string password)
{
	this->info.name = name;
	this->info.serverUri = serverUri;
	this->info.database = database;
	this->info.username = username;
	this->info.password = password;

	this->driver = NULL;
	this->connection = NULL;
	this->enableThread = false;
	this->threadRunning = false;
}
Connection::~Connection(void)
{
	this->mutex.lock();
	this->enableThread = false;
	this->mutex.unlock();
	while (this->threadRunning) Sleep(100);

	this->mutex.lock();
	for (auto& it : this->sqlOperations)
		delete it.second;
	this->sqlOperations.clear();
	this->mutex.unlock();

	if (isConnected())
		this->closeConnection();
}
inline std::string Connection::getName(void)
{
	return this->info.name;
}
void Connection::resetAccessTime(void)
{
	this->mutex.lock();
	this->lastAccess = time(NULL);
	this->mutex.unlock();
}
void Connection::runConnectionKillThread(unsigned long maxTimeout)
{
	if (this->threadRunning)
		return;
	this->enableThread = true;
	resetAccessTime();
	std::thread t = std::thread(&Connection::thread_connectionWatch, maxTimeout, *this);
	t.detach();
}
void Connection::stopConnectionKillThread(bool blockUntilStopped)
{
	if (!this->enableThread && !blockUntilStopped)
		return;
	this->mutex.lock();
	this->enableThread = false;
	this->mutex.unlock();
	if (blockUntilStopped)
		while (this->threadRunning) Sleep(100);
}
void Connection::thread_connectionWatch(unsigned long maxTimeout, Connection& object)
{
	object.threadRunning = true;
	while (true)
	{
		for (int i = 0; i < CONNECTIONKILL_SLEEPCOUNT; i++)
		{
			Sleep(CONNECTIONKILL_SLEEPTIME);
			if (!object.enableThread)
				break;
		}
		if (!object.enableThread)
			break;
		if (time(NULL) - object.lastAccess < maxTimeout)
		{
			object.closeConnection();
			break;
		}
	}
	object.threadRunning = false;
}

bool Connection::isConnected(void)
{
	return this->connection != NULL;
}
void Connection::openConnection(unsigned int maxTimeout)
{
	if (this->isConnected())
		return;
	this->mutex.lock();
	try
	{

		this->connection = this->driver->connect(this->info.serverUri, this->info.username, this->info.password);
		if (maxTimeout > 0)
			runConnectionKillThread(maxTimeout);
	}
	catch (sql::SQLException e)
	{
		this->mutex.unlock();
		throw sql::SQLException(std::string("Could not connect to database. Error message: ").append(e.what()).c_str());
	}
	catch (std::invalid_argument e)
	{
		this->mutex.unlock();
		throw std::invalid_argument(std::string("Cannot interpret argument as UNSIGNED INT. Error message: ").append(e.what()).c_str());
	}
	catch (std::out_of_range e)
	{
		this->mutex.unlock();
		throw std::out_of_range(std::string("Provided value is out of range. Error message: ").append(e.what()).c_str());
	}
	catch (std::exception e)
	{
		this->mutex.unlock();
		throw std::exception(std::string("Unknown Error. Error message: ").append(e.what()).c_str());
	}
	this->mutex.unlock();
}
void Connection::closeConnection(void)
{
	if (!this->isConnected())
		return;
	this->mutex.lock();
	this->connection->close();
	delete this->connection;
	this->connection = NULL;
	this->mutex.unlock();
}

const std::string Connection::createOperationSet(const std::string& keyword)
{
	this->mutex.lock();
	if (!this->isConnected())
	{
		this->mutex.unlock();
		throw std::exception("Connection is closed");
	}
	if (this->sqlOperations.find(keyword) != this->sqlOperations.end())
	{
		this->mutex.unlock();
		throw std::exception("Keyword already in use");
	}
	sql::Statement* stmnt;
	try
	{
		stmnt = this->connection->createStatement();
	}
	catch (sql::SQLException e)
	{
		this->mutex.unlock();
		throw sql::SQLException(std::string("Error while creating OperationSet: ").append(e.what()).c_str());
	}
	
	stmnt->execute(std::string("use ").append(this->info.database));
	this->sqlOperations.insert(std::pair<std::string, SQLOPERATION*>(keyword, new SQLOPERATION(stmnt)));
	//this->sqlOperations[keyword] = SQLOPERATION(stmnt);
	this->mutex.unlock();
	return keyword;
}
void Connection::closeOperationSet(const std::string& keyword)
{
	this->mutex.lock();
	if (!this->isConnected())
	{
		this->mutex.unlock();
		throw std::exception("Connection is closed");
	}
	auto pair = this->sqlOperations.find(keyword);
	if (pair == this->sqlOperations.end())
	{
		this->mutex.unlock();
		throw std::exception("Keyword is not existing");
	}
	SQLOPERATION* op = this->sqlOperations[keyword];
	op->close();
	this->sqlOperations.erase(keyword);
	delete op;
	this->mutex.unlock();
}

void Connection::executeQuery(const std::string& keyword, const std::string& query)
{
	this->mutex.lock();
	auto pair = this->sqlOperations.find(keyword);
	if (!this->isConnected())
	{
		this->mutex.unlock();
		throw std::exception("Connection is closed");
	}
	if (pair == this->sqlOperations.end())
	{
		this->mutex.unlock();
		throw std::exception("Keyword is not existing");
	}
	if (pair->second->resultSet != NULL)
	{
		this->mutex.unlock();
		throw std::exception("Operation still in use by a Query");
	}
	try
	{
		pair->second->resultSet = pair->second->statement->executeQuery(query);
	}
	catch (sql::SQLException e)
	{
		this->mutex.unlock();
		throw sql::SQLException(std::string("Error while executing query: ").append(e.what()).c_str());
	}
	this->mutex.unlock();
}
bool Connection::execute(const std::string& keyword, const std::string& statement)
{
	this->mutex.lock();
	auto pair = this->sqlOperations.find(keyword);
	if (!this->isConnected())
	{
		this->mutex.unlock();
		throw std::exception("Connection is closed");
	}
	if (pair == this->sqlOperations.end())
	{
		this->mutex.unlock();
		throw std::exception("Keyword is not existing");
	}
	if (pair->second->resultSet != NULL)
	{
		this->mutex.unlock();
		throw std::exception("Operation still in use by a Query");
	}
	bool result;
	try
	{
		result = pair->second->statement->execute(statement);
	}
	catch (sql::SQLException e)
	{
		this->mutex.unlock();
		throw sql::SQLException(std::string("Error while executing query: ").append(e.what()).c_str());
	}
	this->mutex.unlock();
	return result;
}
int Connection::executeUpdate(const std::string& keyword, const std::string& update)
{
	this->mutex.lock();
	if (!this->isConnected())
	{
		this->mutex.unlock();
		throw std::exception("Connection is closed");
	}
	auto pair = this->sqlOperations.find(keyword);
	if (pair == this->sqlOperations.end())
	{
		this->mutex.unlock();
		throw std::exception("Keyword is not existing");
	}
	if (pair->second->resultSet != NULL)
	{
		this->mutex.unlock();
		throw std::exception("Operation still in use by a Query");
	}
	int result;
	try
	{
		result = pair->second->statement->executeUpdate(update);
	}
	catch (sql::SQLException e)
	{
		this->mutex.unlock();
		throw sql::SQLException(std::string("Error while executing query: ").append(e.what()).c_str());
	}
	this->mutex.unlock();
	return result;
}

void Connection::query_clear(const std::string& keyword)
{
	this->mutex.lock();
	auto pair = this->sqlOperations.find(keyword);
	if (pair == this->sqlOperations.end())
	{
		this->mutex.unlock();
		throw std::exception("Keyword is not existing");
	}
	if (pair->second->resultSet == NULL)
	{
		this->mutex.unlock();
		return;
	}
	pair->second->buffer.clear();
	pair->second->resultSet->close();
	delete pair->second->resultSet;
	pair->second->resultSet = NULL;
	this->mutex.unlock();
}
void Connection::query_next(const std::string& keyword, char* output, size_t outputSize)
{
	this->mutex.lock();
	auto pair = this->sqlOperations.find(keyword);
	if (pair == this->sqlOperations.end())
	{
		this->mutex.unlock();
		throw std::exception("Keyword is not existing");
	}

	if (pair->second->resultSet == NULL)
	{
		this->mutex.unlock();
		throw std::exception("No previous query existing");
	}
	if (pair->second->buffer.peek() == EOF)
	{//Load a new row into the stringstream
		if (pair->second->resultSet->next())
		{
			pair->second->buffer.clear();
			pair->second->buffer << '[';
			int cellIndex = 1;
			try
			{
				auto columnCount = pair->second->resultSet->getMetaData()->getColumnCount();
				for (unsigned int i = 1; i <= columnCount; i++)
				{
					std::string res = pair->second->resultSet->getString(cellIndex);
					if (cellIndex > 1)
						pair->second->buffer << ",\"" << res << '"';
					else
						pair->second->buffer << '"' << res << '"';
					cellIndex++;
				}
			}
			catch (std::exception e)
			{
				this->mutex.unlock();
				throw std::exception(std::string("Error while reading new row from ResultSet: ").append(e.what()).c_str());
			}
			pair->second->buffer << ']';
		}
		else
		{
			pair->second->resultSet->close();
			delete pair->second->resultSet;
			pair->second->resultSet = NULL;
			this->mutex.unlock();
			return;
		}
	}
	std::string result;
	pair->second->buffer.readsome(output, outputSize - 1);
	output[pair->second->buffer.gcount()] = '\0';
}
static unsigned int keywordCount = 0;
inline std::string Connection::getPseudoUniqueKeyword()
{
	return std::string("CONOPKEYWORD").append(std::to_string(keywordCount++));
}