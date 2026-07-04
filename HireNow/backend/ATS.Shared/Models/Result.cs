using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ATS.Shared.Models
{
    public class Result
    {
        protected Result(bool isSuccess, string error, IEnumerable<string> errors = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            Errors = errors ?? new List<string>();
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        [JsonPropertyName("message")]
        public string Error { get; }
        public IEnumerable<string> Errors { get; }

        public static Result Success() => new Result(true, string.Empty);
        public static Result Failure(string error) => new Result(false, error);
        public static Result Failure(IEnumerable<string> errors) => new Result(false, "One or more validation failures occurred.", errors);

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
        public static Result<T> Failure<T>(IEnumerable<string> errors) => Result<T>.Failure(errors);
    }

    public class Result<T> : Result
    {
        private readonly T _value;

        protected Result(T value, bool isSuccess, string error, IEnumerable<string> errors = null)
            : base(isSuccess, error, errors)
        {
            _value = value;
        }

        [JsonPropertyName("data")]
        public T Value => _value;

        public static Result<T> Success(T value) => new Result<T>(value, true, string.Empty);
        public new static Result<T> Failure(string error) => new Result<T>(default, false, error);
        public new static Result<T> Failure(IEnumerable<string> errors) => new Result<T>(default, false, "One or more validation failures occurred.", errors);
    }
}
