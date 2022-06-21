test | As a user, I can give bob a nice message
	print a nice message for bob

test | As a horrible user, I can give bob a horrible message
	print a horrible message for bob

func | print a nice message for bob
	say hello to bob

func | print a horrible message for bob
	say go away to bob

func | say hello to bob
	echo | hello, bob

func | say go away to bob
	echo | go away, bob

