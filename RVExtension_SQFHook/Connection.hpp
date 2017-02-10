#pragma once
#include <string>
#include <map>
#include <mutex>
#include <sstream>

#define CONNECTIONKILL_SLEEPTIME 1 * 1000
#define CONNECTIONKILL_SLEEPCOUNT 20


class Connection
{
private:
	typedef struct structSqlOperation
	{
		sql::Statement*			statement;
		sql::ResultSet*			resultSet;
		std::stringstream&		buffer;
		structSqlOperation(sql::Statement* Statement = NULL) : statement(Statement), buffer(std::stringstream()) { this->resultSet = NULL; }
		structSqlOperation(structSqlOperation& that) = delete;
		~structSqlOperation(void) { this->close(); }
		void close(void)
		{
			this->buffer.clear();
			if (this->resultSet != NULL)
			{
				this->resultSet->close();
				delete this->resultSet;
				this->resultSet = NULL;
			}
			if (this->statement != NULL)
			{
				this->statement->close();
				delete this->statement;
				this->statement = NULL;
			}
		}
	}SQLOPERATION;
	typedef struct structDatabaseInfo
	{
		std::string name;
		std::string username;
		std::string password;
		std::string serverUri;
		std::string database;
	}DATABASEINFO;

	DATABASEINFO info;
	sql::Connection* connection;
	sql::Driver* driver;
	std::map<std::string, SQLOPERATION*> sqlOperations;
	time_t lastAccess;
	std::mutex mutex;

	bool enableThread;
	bool threadRunning;



public:
	Connection(std::string name, std::string serverUri, std::string database, std::string username, std::string password = "");
	Connection(const Connection& that)
	{
		throw std::exception("copy not allowed");
	}
	~Connection(void);


	std::string getName(void);
	/*
	 Resets the last access for the ConnectionKill thread
	*/
	void resetAccessTime(void);
	/*
	 Starts the ConnectionKillThread

	 @Param maxTimeout				maximum timeout (in milli seconds) the timer should wait before closing the connection
	*/
	void runConnectionKillThread(unsigned long maxTimeout);
	/*
	 Stops the ConnectionKillThread

	 @Param blockUntilStopped		defines wether or not the function call should block until the thread was stopped
	*/
	void stopConnectionKillThread(bool blockUntilStopped = false);

	/*
	 Checks if this Connection object is connected

	 @Return						true if this object is connected, false if it is not
	*/
	bool isConnected(void);
	/*
	 Opens a connection to the target represented by this Connection object

	 @param	maxTimeout				max timeout for the connection watch thread
	*/
	void openConnection(unsigned int maxTimeout = 0);
	/*
	 Closes an active connection which is represented by this Connection object

	 @Param maxTimeout				if not 0, will start ConnectionKill thread with given timeout value (see runConnectionKillThread function)

	 @throw	sql::SQLException		
			std::invalid_argument	
			std::out_of_range		
			std::exception			
	*/
	void closeConnection(void);

	/*
	 Creates a new OperationSet with given keyword

	 @Param	keyword					the keyword to use for the new result set
	 @Return						the keyword used for the new OperationSet

	 @Throw std::exception			if the keyword is already used OR the connection was not established
			sql::SQLException
	 */
	const std::string createOperationSet(const std::string& keyword = getPseudoUniqueKeyword());
	/*
	 Closes an existing OperationSet that matches the keyword
	 
	 @Param keyword					KeyWord to identifie the OperationSet to operate on
	 
	 @Throw std::exception			if the keyword is not existing OR the connection is not established anymore (thus OperationSets are closed)
	*/
	void closeOperationSet(const std::string& keyword);

	/*
	 Executes given query on given OperationSet (keyword)

	 @Param	keyword				KeyWord to identifie the OperationSet to operate on
	 @Param	query				The query to execute

	 @Throw	std::exception		keyword not existing OR OperationSet still in use OR the connection is not established anymore
	*/
	void executeQuery(const std::string& keyword, const std::string& query);
	/*
	 Executes given statement on given OperationSet (keyword)
	 
	 @Param	keyword				KeyWord to identifie the OperationSet to operate on
	 @Param	statement			The statement to execute
	 @Return					true/false depending on if the statement was executed successfully
	 
	 @Throw	std::exception		keyword not existing OR OperationSet still in use OR the connection is not established anymore
			sql::SQLException	
	*/
	bool execute(const std::string& keyword, const std::string& update);
	/*
	 Executes given update on given OperationSet (keyword)
	 
	 @Param	keyword				KeyWord to identifie the OperationSet to operate on
	 @Param	statement			The statement to execute
	 @Return					Rows affected by this update
	 
	 @Throw	std::exception		keyword not existing OR OperationSet still in use OR the connection is not established anymore
			sql::SQLException	
	*/
	int executeUpdate(const std::string& keyword, const std::string& update);

	void query_clear(const std::string& keyword);
	void query_next(const std::string& keyword, char* output, size_t outputSize);
	
private:
	/*
	 Thread function, not supposed to be callen programmaticly
	*/
	static void thread_connectionWatch(unsigned long maxTimeout, Connection& object);
	/*
	 Returns PseudoUnique keywords for OperationSets
	 If a keyword is existing with following regex: CONOPKEYWORD[0-9]*
	 then the function can generate duplicates
	 */
	static std::string getPseudoUniqueKeyword();
};

