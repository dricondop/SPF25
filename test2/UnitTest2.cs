using Xunit;
using System;

public class FunctionalTests
{
    [Fact]
    public void Withdraw_NegativeAmount_ShouldThrowException()
    {
        //arrange
        var account = new PROJECT.Program.BankAccount(12345, "John Doe", 1000m);

        //act & assert
        var exception = Assert.Throws<ArgumentException>(() => account.Withdraw(-100m));

        //verify exception message
        Assert.Equal("Withdrawal amount must be greater than zero.", exception.Message);

        //ensure balance remains unchanged
        Assert.Equal(1000m, account.Balance);
    }
}
