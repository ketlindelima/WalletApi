using FluentAssertions;
using WalletApi.Models;
using Xunit;

namespace WalletApi.Tests;

public class AccountTests
{
    [Fact] // Toda conta inicia com saldo zero
    public void CreateAccount_ShouldStartWithZeroBalance()
    {
        
        var account = new Account();

        account.Balance.Should().Be(0);
    }

    [Fact]
    public void Credit_ShouldIncreaseBalance()
    {
        var account = new Account();

        account.Credit(100);

        account.Balance.Should().Be(100);
    }

    [Fact]
    public void Debit_ShouldDecreaseBalance()
    {
        var account = new Account();

        account.Credit(100);

        account.Debit(40);

        account.Balance.Should().Be(60);
    }

    [Fact]
    public void Debit_WithInsufficientBalance_ShouldThrow()
    {
        var account = new Account();

        var action = () => account.Debit(100);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Saldo insuficiente.");
    }

    [Fact]
    public void Transfer_ShouldMoveMoneyBetweenAccounts()
    {
        var from = new Account();
        var to = new Account();

        from.Credit(100);

        from.TransferOut(30);
        to.TransferIn(30);

        from.Balance.Should().Be(70);
        to.Balance.Should().Be(30);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Credit_WithInvalidAmount_ShouldThrow(decimal amount)
    {
        var account = new Account();

        var action = () => account.Credit(amount);

        action.Should()
            .Throw<ArgumentException>();
    }

}