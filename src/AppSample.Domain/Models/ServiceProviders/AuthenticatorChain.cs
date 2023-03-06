using System.Collections;

namespace AppSample.Domain.Models.ServiceProviders;

/// <summary>
/// Цепочка аутентификаторов
/// </summary>
public struct AuthenticatorChain : IEnumerable<AuthenticatorEntity>
{
    readonly AuthenticatorEntity[]? _authenticatorsSorted;
    public AuthenticatorChain(AuthenticatorEntity[]? authenticators)
    {
        _authenticatorsSorted = authenticators?
            .OrderBy(i => i.OrderLevel1)
            .ThenBy(i => i.OrderLevel2)
            .ToArray();

        PreviousStarted = default;
    }

    public AuthenticatorEntity PreviousStarted { get; set; }
    public AuthenticatorEntity? GetFirst => _authenticatorsSorted?.FirstOrDefault();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<AuthenticatorEntity> GetEnumerator() => new Enumerator(_authenticatorsSorted, PreviousStarted);

    struct Enumerator : IEnumerator<AuthenticatorEntity>
    {
        LinkedList<AuthenticatorEntity>.Enumerator _authenticatorsEnumerator;

        /// <summary>
        /// Перечисление начинается со следующей группы аутентификаторов
        /// </summary>
        /// <param name="authenticators"></param>
        /// <param name="previousStarted"></param>
        public Enumerator(IEnumerable<AuthenticatorEntity>? authenticators, AuthenticatorEntity? previousStarted)
        {
            var sortedAuthenticators = authenticators?
                .Where(a => previousStarted == null || a.OrderLevel1 > previousStarted.OrderLevel1);

            var authenticatorsLinkedList = sortedAuthenticators != null ?
                new LinkedList<AuthenticatorEntity>(sortedAuthenticators) :
                new LinkedList<AuthenticatorEntity>();

            _authenticatorsEnumerator = authenticatorsLinkedList.GetEnumerator();
        }

        public bool MoveNext() => _authenticatorsEnumerator.MoveNext();

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public AuthenticatorEntity Current => _authenticatorsEnumerator.Current;
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _authenticatorsEnumerator.Dispose();
        }
    }
}
