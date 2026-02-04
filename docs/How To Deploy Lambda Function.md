# How to deploy lambda function

This document assumes you already have a local lambda function code you would like to deploy. If you do not have local lambda function code, please make one. (insert docs how later) This document also assumes you have already installed dotnet and AWS's CLI and Lambda CLI tools.

This document is based on this [AWS article](https://docs.aws.amazon.com/lambda/latest/dg/csharp-package-cli.html).

1) `cd` to directory where your {your lambda function name here}.csproj is 
2) configure AWS creditials if you have not

    2.1) run `aws configure` in the command line
    
    2.2) input your access key (get them from Sam)

    2.3) set region to `us-east-2`

    2.4) dont enter anything and just press enter for "Default output format"

    2.5) you can verify wether your default profile now has the keys by running `aws configure`

3) run `dotnet lambda deploy-function {your lambda function name here}`

4) when prompted for IAM Role for the Lambda, enter the number that corresponds to `lambda_basic_role` (or other one if you know you need it)

You can run `dotnet lambda invoke-function {your lambda function name here} --payload {your payload here}` to run your lambda function
    