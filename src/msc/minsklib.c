#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

extern void printInt(long number) 
{ 
  printf("%ld\n", number); 
}

extern void printText(char *text) 
{ 
  printf("%s\n", text); 
}

clock_t t;
extern void start_timer()
{
  t = clock();
}

extern void stop_timer()
{
  t = clock() - t;
  double time_taken = ((double)t)/CLOCKS_PER_SEC;
  printf("elapsed time %f seconds", time_taken);
}

extern char* readInput()
{
#define CHUNK 20
   char* input = NULL;
   char tempbuf[CHUNK];
   size_t inputlen = 0, templen = 0;
   do {
       fgets(tempbuf, CHUNK, stdin);
       templen = strlen(tempbuf);
       input = realloc(input, inputlen+templen+1);
       strcpy(input+inputlen, tempbuf);
       inputlen += templen;
    } while (templen==CHUNK-1 && tempbuf[CHUNK-2]!='\n');

    input[inputlen - 1] = '\0';

    return input;
}

char true_string[] = "true\0";
char false_string[] = "false\0";

extern char* convert_bool_to_string(bool v)
{
  if(v)
    return true_string;
  else
    return false_string;
}

char string_buffer[128];
extern char* convert_int_to_string(long long v)
{
  sprintf(string_buffer, "%lld", v);
  return string_buffer;
}

extern long long convert_string_to_int(char* str)
{
  return atoll(str);
}

time_t t = 0;

extern int minsk_rand(int limit) {

  if (t == 0)
  {
    t = clock();
    srand((unsigned)time(&t));
  }

  return rand() % limit;
}