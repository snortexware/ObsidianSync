
namespace G.Sync.Repository.Common
{

public class TransactionContext : ITransaction
{
	private readonly IDbConnection _connection;
	private readonly IDbTransaction _transaction;
	private bool _disposed = false;
	private bool _committed = false;
	public IDbConnection Connection => _connection;
	public IDbTransaction Transaction => _transaction;

	public TransactionContext()
	{
		_connection = DataBaseContext.Instance.GetConnection();
		_transaction = _connection.BeginTransaction();
	}
	/// <summary>
	/// Confirma a transação.
	/// </summary>
	public void Complete()
	{
		_transaction.Commit();
		_committed = true;
	}
	/// <summary>
	/// Desfaz a transação.
	/// </summary>
	public void Rollback()
	{
		_transaction.Rollback();
	}
	public void Dispose()
	{
		if (!_disposed)
		{
			if (!_committed)
			{
				_transaction.Rollback();
			}
			_transaction.Dispose();
			_connection.Dispose();
			_disposed = true;
		}
	}
}
}