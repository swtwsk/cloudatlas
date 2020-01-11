using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Shared;
using Shared.Monads;
using Shared.RPC;
using Shared.Serializers;
using Attribute = Shared.Model.Attribute;

namespace QuerySigner
{
    using EitherResponse = Either<RefStruct<SignError>, SignedQuery>;

    public class QueryServer : IDisposable
    {
        private readonly Thread _thread;
        private readonly Server _server;
        private readonly RSA _rsa;

        private readonly ISet<string> _queries = new HashSet<string>();

        public QueryServer(ServerPort serverPort, RSA rsa)
        {
            _server = new Server
            {
                Ports = {serverPort},
                Services =
                {
                    ServerServiceDefinition.CreateBuilder()
                        .AddMethod(SignerMethods.SignQuery, SignQuery)
                        .AddMethod(SignerMethods.UnsignQuery, UnsignQuery)
                        .Build()
                }
            };
            _rsa = rsa;
            _thread = new Thread(_server.Start);
            _thread.Start();
        }

        public void Dispose()
        {
            _thread?.Interrupt();
            _server?.ShutdownAsync();
        }

        private static readonly ISet<string> ReservedAttributes = new HashSet<string>
        {
            "level", "name", "owner", "timestamp", "contacts", "update", "cardinality"
        };

        private Task<EitherResponse> SignQuery(SignRequest request, ServerCallContext context)
        {
            var (query, name) = request;

            lock (_queries)
            {
                if (_queries.Contains(name))
                    return Task.FromResult(EitherResponse.Left(new RefStruct<SignError>(SignError.ConflictingQuery)));

                if (!Attribute.IsProperName(name) || !Attribute.IsQuery(name))
                    return Task.FromResult(EitherResponse.Left(new RefStruct<SignError>(SignError.IncorrectName)));

                try
                {
                    if (!Interpreter.Interpreter.TryParse(query, out _))
                        return Task.FromResult(EitherResponse.Left(new RefStruct<SignError>(SignError.IncorrectQuery)));
                }
                catch (Exception)
                {
                    return Task.FromResult(EitherResponse.Left(new RefStruct<SignError>(SignError.IncorrectQuery)));
                }

                _queries.Add(name);
            }

            var serialized = CustomSerializer.Serializer.Serialize(request);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(serialized);
            var encryptedHash = _rsa.Encrypt(hash, RSAEncryptionPadding.Pkcs1);
            return Task.FromResult(EitherResponse.Right(new SignedQuery
            {
                SerializedData = serialized,
                HashSign = encryptedHash
            }));
        }

        private Task<RefStruct<bool>> UnsignQuery(string name, ServerCallContext context)
        {
            bool queryRemoved;

            lock (_queries)
                queryRemoved = _queries.Remove(name);

            return Task.FromResult(queryRemoved.ToNullableWrapper());
        }
    }
}
