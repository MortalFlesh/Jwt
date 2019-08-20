﻿// Copyright (c) 2018 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for more information.

namespace JsonWebToken.Internal
{
    /// <summary>
    /// Represents a <see cref="IValidator"/> verifying the JWT has a required claim.
    /// </summary>
    internal class RequiredIntegerClaimValidator : IValidator
    {
        private readonly string _claim;
        private readonly long? _value;

        /// <summary>
        /// Initializes an instance of <see cref="RequiredIntegerClaimValidator"/>.
        /// </summary>
        /// <param name="claim"></param>
        public RequiredIntegerClaimValidator(string claim)
            : this(claim, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="RequiredIntegerClaimValidator"/>.
        /// </summary>
        /// <param name="claim"></param>
        /// <param name="value"></param>
        public RequiredIntegerClaimValidator(string claim, long? value)
        {
            if (claim is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.claim);
            }

            _claim = claim!;
            _value = value;
        }

        /// <inheritdoc />
        public TokenValidationResult TryValidate(Jwt jwt)
        {
            if (jwt is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jwt);
            }

            if (jwt!.Payload is null)
            {
                return TokenValidationResult.MalformedToken();
            }

            var claim = jwt.Payload[_claim];
            if (claim is null)
            {
                return TokenValidationResult.MissingClaim(jwt, _claim);
            }

            if (_value != (long?)claim)
            {
                return TokenValidationResult.InvalidClaim(jwt, _claim);
            }

            return TokenValidationResult.Success(jwt);
        }
    }

     /// <summary>
    /// Represents a <see cref="IValidator"/> verifying the JWT has a required claim.
    /// </summary>
    internal class RequiredDoubleClaimValidator : IValidator
    {
        private readonly string _claim;
        private readonly double? _value;

        /// <summary>
        /// Initializes an instance of <see cref="RequiredDoubleClaimValidator"/>.
        /// </summary>
        /// <param name="claim"></param>
        public RequiredDoubleClaimValidator(string claim)
            : this(claim, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="RequiredDoubleClaimValidator"/>.
        /// </summary>
        /// <param name="claim"></param>
        /// <param name="value"></param>
        public RequiredDoubleClaimValidator(string claim, double? value)
        {
            if (claim is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.claim);
            }

            _claim = claim!;
            _value = value;
        }

        /// <inheritdoc />
        public TokenValidationResult TryValidate(Jwt jwt)
        {
            if (jwt is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jwt);
            }

            if (jwt!.Payload is null)
            {
                return TokenValidationResult.MalformedToken();
            }

            var claim = jwt.Payload[_claim];
            if (claim is null)
            {
                return TokenValidationResult.MissingClaim(jwt, _claim);
            }

            if (_value != (double?)claim)
            {
                return TokenValidationResult.InvalidClaim(jwt, _claim);
            }

            return TokenValidationResult.Success(jwt);
        }
    }


    /// <summary>
    /// Represents a <see cref="IValidator"/> verifying the JWT has a required claim.
    /// </summary>
    internal class RequiredStringClaimValidator : IValidator
    {
        private readonly string _claim;
        private readonly string _value;

        /// <summary>
        /// Initializes an instance of <see cref="RequiredStringClaimValidator"/>.
        /// </summary>
        /// <param name="claim"></param>
        /// <param name="value"></param>
        public RequiredStringClaimValidator(string claim, string value)
        {
            if (claim is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.claim);
            }

            if (value is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }

            _claim = claim!;
            _value = value!;
        }

        /// <inheritdoc />
        public TokenValidationResult TryValidate(Jwt jwt)
        {
            if (jwt is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jwt);
            }

            if (jwt!.Payload is null)
            {
                return TokenValidationResult.MalformedToken();
            }

            var claim = jwt.Payload[_claim];
            if (claim is null)
            {
                return TokenValidationResult.MissingClaim(jwt, _claim);
            }

            if (!string.Equals(_value, (string?)claim))
            {
                return TokenValidationResult.InvalidClaim(jwt, _claim);
            }

            return TokenValidationResult.Success(jwt);
        }
    }

    /// <summary>
    /// Represents a <see cref="IValidator"/> verifying the JWT has a required claim.
    /// </summary>
    internal class RequiredClaimValidator : IValidator
    {
        private readonly string _claim;

        /// <summary>
        /// Initializes an instance of <see cref="RequiredClaimValidator"/>.
        /// </summary>
        /// <param name="claim"></param>
        public RequiredClaimValidator(string claim)
        {
            _claim = claim;
        }

        /// <inheritdoc />
        public TokenValidationResult TryValidate(Jwt jwt)
        {
            if (jwt is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jwt);
            }

            if (jwt!.Payload is null)
            {
                return TokenValidationResult.MalformedToken();
            }

            if (jwt.Payload.ContainsKey(_claim))
            {
                return TokenValidationResult.Success(jwt);
            }

            return TokenValidationResult.MissingClaim(jwt, _claim);
        }
    }
}
