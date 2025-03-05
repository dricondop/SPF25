using Xunit;

public class BankAccountTests
{
    [Fact]
    public void Withdraw_ValidAmount_ShouldReduceBalance()
    {
        //arrange
        var account = new PROJECT.Program.BankAccount(12345, "John Doe", 1000m);

        //act
        bool result = account.Withdraw(200m);

        //assert
        Assert.True(result);    //Withdrawal should be successful
        Assert.Equal(800m, account.Balance);    //Balance should be reduced by the amount withdrawn
    }
}
